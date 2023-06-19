using Lib.Common;
using Lib.Common.Data.DAO;
using RedminePublisher.Data;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text;

namespace RedminePublisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LoadEnvs();
            IHostBuilder builder = Host.CreateDefaultBuilder(args);
            IConfiguration configuration = LoadConfiguration(builder);
            RedminePublisherConfigs configs = MakeCustomConfigs(configuration);
            var loggerConfiguration = ConfigureNPLogger(configs);
            Log.Logger = loggerConfiguration.CreateLogger();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(configs);
                services.AddHostedService<IssuePublisher>();
            });
            IHost host = builder.Build();
            try
            {
                if (CheckApiConnection(ref configs))
                    await host.RunAsync();
            }
            catch(HttpRequestException ex)
            {
                Log.Logger.Error("Service start failed. {error_message}", ex.Message);
            }
            catch(Exception ex)
            {
                Log.Logger.Error("Service start failed. {error_message}", ex.Message);
            }
        }
        private static void LoadEnvs()
        {
            #region CommonConsts
            string esUrl = Environment.GetEnvironmentVariable(CommonConsts.ENV_NAME_ES_URL);
            string esUser = Environment.GetEnvironmentVariable(CommonConsts.ENV_NAME_ES_USER);
            string esPass = Environment.GetEnvironmentVariable(CommonConsts.ENV_NAME_ES_PASS);
            if (string.IsNullOrWhiteSpace(esUrl))
                Environment.SetEnvironmentVariable(CommonConsts.ENV_NAME_ES_URL, "http://host.docker.internal:9200/");
            if (string.IsNullOrWhiteSpace(esUser))
                Environment.SetEnvironmentVariable(CommonConsts.ENV_NAME_ES_USER, "elastic");
            if (string.IsNullOrWhiteSpace(esPass))
                Environment.SetEnvironmentVariable(CommonConsts.ENV_NAME_ES_PASS, "changeme");
            #endregion CommonConsts

            #region API
            string npApiUrl = Environment.GetEnvironmentVariable(Consts.ENV_NAME_NP_API_URL);
            string npApiKey = Environment.GetEnvironmentVariable(Consts.ENV_NAME_NP_API_KEY);
            if (string.IsNullOrWhiteSpace(npApiUrl))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_NP_API_URL, "http://localhost:5080/");
            if (string.IsNullOrWhiteSpace(npApiKey))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_NP_API_KEY, "redmineapisecretkey");
            #endregion API

            #region Notion
            string notionApiVersion = Environment.GetEnvironmentVariable(Consts.ENV_NAME_NOTION_API_VERSION);
            if (string.IsNullOrWhiteSpace(notionApiVersion))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_NOTION_API_VERSION, "2022-06-28");
            #endregion
        }
        static IConfiguration LoadConfiguration(IHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
#else
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#endif
                .Build();

            // Overwrite configs for custom appsettings.
            string esUri = configuration["ElasticSearch:Uri"];
            if (!string.IsNullOrWhiteSpace(esUri))
                configuration[CommonConsts.ENV_NAME_ES_URL] = esUri;
            string esUser = configuration["ElasticSearch:User"];
            if (!string.IsNullOrWhiteSpace(esUser))
                configuration[CommonConsts.ENV_NAME_ES_USER] = esUser;
            string esPass = configuration["ElasticSearch:Pass"];
            if (!string.IsNullOrWhiteSpace(esPass))
                configuration[CommonConsts.ENV_NAME_ES_PASS] = esPass;

            string npApiUrl = configuration["NPApi:Url"];
            if (!string.IsNullOrWhiteSpace(npApiUrl))
                configuration[Consts.ENV_NAME_NP_API_URL] = npApiUrl;
            string npApiKey = configuration["NPApi:ApiKey"];
            if (!string.IsNullOrWhiteSpace(npApiKey))
                configuration[Consts.ENV_NAME_NP_API_KEY] = npApiKey;

            string notionApiVersion = configuration["Notion:ApiVersion"];
            if (!string.IsNullOrWhiteSpace(notionApiVersion))
                configuration[Consts.ENV_NAME_NOTION_API_VERSION] = notionApiVersion;

            return configuration;
        }
        private static RedminePublisherConfigs MakeCustomConfigs(IConfiguration configuration)
        {
            RedminePublisherConfigs configs = new RedminePublisherConfigs(
                configuration[CommonConsts.ENV_NAME_ES_URL],
                configuration[CommonConsts.ENV_NAME_ES_USER],
                configuration[CommonConsts.ENV_NAME_ES_PASS],
                configuration[Consts.ENV_NAME_NP_API_URL],
                configuration[Consts.ENV_NAME_NP_API_KEY],
                configuration[Consts.ENV_NAME_NOTION_API_VERSION]
            );
            return configs;
        }
        private static LoggerConfiguration ConfigureNPLogger(RedminePublisherConfigs configs)
        {
            string projectName = Assembly.GetEntryAssembly().GetName().Name;
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .WriteTo.File(new ElasticsearchJsonFormatter(), $"logs/np-{projectName}-.log", rollingInterval: RollingInterval.Day);
            if (!(string.IsNullOrWhiteSpace(configs.EsUrl) || string.IsNullOrWhiteSpace(configs.EsUser) || string.IsNullOrWhiteSpace(configs.EsPass)))
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configs.EsUrl))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"np-{projectName}-{{0:yyyy.MM.dd}}",
                    CustomFormatter = new ElasticsearchJsonFormatter(),
                    ModifyConnectionSettings = c => c.BasicAuthentication(configs.EsUser, configs.EsPass)
                });
            }
            return loggerConfiguration;
        }
        private static bool CheckApiConnection(ref RedminePublisherConfigs configs)
        {
            UserDto userDao = null;
            // Try to connect to NP API.
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, $"{configs.NpApiUrl}api/user/login"))
                {
                    reqMsg.Headers.Add(CommonConsts.NAME_REQ_HEADER_NP_API_KEY, configs.NpApiKey);
                    string serializedUserDao = JsonSerializer.Serialize(new UserDto(configs.NpApiKey));
                    reqMsg.Content = new StringContent(serializedUserDao, Encoding.UTF8, "application/json");

                    var response = client.Send(reqMsg, CancellationToken.None);
                    if (response.IsSuccessStatusCode)
                    {
                        userDao = JsonSerializer.Deserialize<UserDto>(response.Content.ReadAsStringAsync().Result);
                        if (userDao.project_keys.Count() == 0)
                        {
                            Log.Logger.Error("Failed to get projects.");
                            return false;
                        }
                    }
                    if (!response.IsSuccessStatusCode || userDao == null)
                    {
                        Log.Logger.Error("Failed to login NPApi.");
                        return false;
                    }
                }
            }

            // Try to connect to Notion API.
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{userDao.nt_api_key}");
                client.DefaultRequestHeaders.Add(Consts.NAME_REQ_HEADER_NOTION_API_VERSION, $"{configs.NotionApiVersion}");
                var response = client.GetAsync($"{Consts.NOTION_API_ENDPOINT_DATABASE}/{userDao.nt_db_id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    Log.Logger.Error("Failed to check Notion DB.");
                    return false;
                }
            }
            configs.UserInfo = userDao;
            return true;
        }
    }
}



