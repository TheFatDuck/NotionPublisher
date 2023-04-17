using Lib.Common;
using Lib.Common.Data.DAO;
using RedmineLoader;
using RedmineLoader.Data;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Text;
using System.Text.Json;

internal class RedmineLoaderMain
{
    public static bool IsDbInitialized = false;
    private static async Task Main(string[] args)
    {
        LoadEnvs();
        IHostBuilder builder = Host.CreateDefaultBuilder(args);
        IConfiguration configuration = LoadConfiguration(builder);
        RedmineLoaderConfigs configs = MakeCustomConfigs(configuration);
        var loggerConfiguration = ConfigureNPLogger(configs);
        Log.Logger = loggerConfiguration.CreateLogger();

        if (CheckApiConnection(ref configs))
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(configs);
                services.AddHostedService<IssueLoader>();
            });
            IHost host = builder.Build();
            await host.RunAsync();
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

        #region Redmine
        string rmUrl = Environment.GetEnvironmentVariable(Consts.ENV_NAME_REDMINE_URL);
        if (string.IsNullOrWhiteSpace(rmUrl))
            Environment.SetEnvironmentVariable(Consts.ENV_NAME_REDMINE_URL, "http://localhost/");
        #endregion Redmine

        #region API
        string apiUrl = Environment.GetEnvironmentVariable(Consts.ENV_NAME_NP_API_URL);
        if (string.IsNullOrWhiteSpace(apiUrl))
            Environment.SetEnvironmentVariable(Consts.ENV_NAME_NP_API_URL, "http://localhost:5080/");
        string apiKey = Environment.GetEnvironmentVariable(Consts.ENV_NAME_NP_API_KEY);
        if (string.IsNullOrWhiteSpace(apiKey))
            Environment.SetEnvironmentVariable(Consts.ENV_NAME_NP_API_KEY, "redmineapisecretkey");
        #endregion API

    }

    private static IConfiguration LoadConfiguration(IHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
#else
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#endif
            .Build();

        // Overwrite configs.
        string esUri = configuration["ElasticSearch:Url"];
        if (!string.IsNullOrWhiteSpace(esUri))
            configuration[CommonConsts.ENV_NAME_ES_URL] = esUri;
        string esUser = configuration["ElasticSearch:User"];
        if (!string.IsNullOrWhiteSpace(esUser))
            configuration[CommonConsts.ENV_NAME_ES_USER] = esUser;
        string esPass = configuration["ElasticSearch:Pass"];
        if (!string.IsNullOrWhiteSpace(esPass))
            configuration[CommonConsts.ENV_NAME_ES_PASS] = esPass;

        string rmUrl = configuration["Redmine:Url"];
        if (!string.IsNullOrWhiteSpace(rmUrl))
            configuration[Consts.ENV_NAME_REDMINE_URL] = rmUrl;

        string apiUrl = configuration["NPApi:Url"];
        if (!string.IsNullOrWhiteSpace(apiUrl))
            configuration[Consts.ENV_NAME_NP_API_URL] = apiUrl;
        string apiKey = configuration["NPApi:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            configuration[Consts.ENV_NAME_NP_API_KEY] = apiKey;

        return configuration;
    }

    private static RedmineLoaderConfigs MakeCustomConfigs(IConfiguration configuration)
    {
        RedmineLoaderConfigs configs = new RedmineLoaderConfigs(
            configuration[CommonConsts.ENV_NAME_ES_URL],
            configuration[CommonConsts.ENV_NAME_ES_USER],
            configuration[CommonConsts.ENV_NAME_ES_PASS],
            configuration[Consts.ENV_NAME_REDMINE_URL],
            configuration[Consts.ENV_NAME_NP_API_URL],
            configuration[Consts.ENV_NAME_NP_API_KEY]
        );
        return configs;
    }

    private static LoggerConfiguration ConfigureNPLogger(RedmineLoaderConfigs configs)
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

    private static bool CheckApiConnection(ref RedmineLoaderConfigs configs)
    {
        UserDao userDao = null;
        // Try to connect to NP API.
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, $"{configs.NpApiUrl}api/user/login"))
            {
                reqMsg.Headers.Add(CommonConsts.NAME_REQ_HEADER_NP_API_KEY, configs.NpApiKey);
                string serializedUserDao = JsonSerializer.Serialize(new UserDao(configs.NpApiKey));
                reqMsg.Content = new StringContent(serializedUserDao, Encoding.UTF8, "application/json");

                var response = client.Send(reqMsg, CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    userDao = JsonSerializer.Deserialize<UserDao>(response.Content.ReadAsStringAsync().Result);
                    if(userDao.project_keys.Count() == 0)
                    {
                        Log.Logger.Error("Failed to get projects.");
                        return false;
                    }
                }
                if(!response.IsSuccessStatusCode || userDao == null)
                {
                    Log.Logger.Error("Failed to login NPApi.");
                    return false;
                }
            }
        }
        // Try to connec to Redmine API.
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(configs.RmUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add(CommonConsts.NAME_REQ_HEADER_RM_API_KEY, userDao.rm_api_key);
            client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
            var response = client.GetAsync("projects.json", CancellationToken.None).Result;
            if (!response.IsSuccessStatusCode)
            {
                Log.Logger.Error("Failed to login to Redmine.");
                return false;
            }
        }

        configs.UserInfo = userDao;
        return true;
    }
}