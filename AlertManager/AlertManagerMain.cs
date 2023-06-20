using Lib.Common;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace AlertManager
{
    public class AlertManagerMain
    {
        private static async Task Main(string[] args)
        {
            LoadEnv();
            IHostBuilder builder = Host.CreateDefaultBuilder(args);
            IConfiguration configuration = LoadConfiguration(builder);
            AlertManagerConfigs configs = MakeCustomConfigs(configuration);
            var loggerConfiguration = ConfigureNPLogger(configs);
            Log.Logger = loggerConfiguration.CreateLogger();

            if(CheckApiConnection(configs))
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(configs);
                    if(!string.IsNullOrEmpty(configs.PromUrl) && !string.IsNullOrEmpty(configs.PromJob))
                        services.AddHostedService<PrometheusChecker>();
                });

                IHost host = builder.Build();
                await host.RunAsync();
            }
        }

        private static void LoadEnv()
        {
            string promUrl = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PROM_URL);
            string promJob = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PROM_JOB);
            if (string.IsNullOrEmpty(promUrl))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PROM_URL, promUrl);
            if(string.IsNullOrEmpty(promJob))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PROM_JOB, promJob);

            string slackAppKey = Environment.GetEnvironmentVariable(Consts.ENV_NAME_SLACK_APP_KEY);
            string slackChannelId = Environment.GetEnvironmentVariable(Consts.ENV_NAME_SLACK_CHANNEL_ID);
            if (string.IsNullOrEmpty(slackAppKey))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_SLACK_APP_KEY, "yourslaskappkeyhere");
            if (string.IsNullOrEmpty(slackChannelId))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_SLACK_CHANNEL_ID, "yourchannelidhere");
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

            string promUrl = configuration["Prometheus:Url"];
            string promJob = configuration["Prometheus:Job"];
            if (!string.IsNullOrEmpty(promUrl))
                configuration[Consts.ENV_NAME_PROM_URL] = promUrl;
            if(!string.IsNullOrEmpty(promJob))
                configuration[Consts.ENV_NAME_PROM_JOB] = promJob;

            string slackAppKey = configuration["Slack:AppKey"];
            string slackChannelId = configuration["Slack:ChannelId"];
            if(!string.IsNullOrEmpty(slackAppKey))
                configuration[Consts.ENV_NAME_SLACK_APP_KEY] = slackAppKey;
            if(!string.IsNullOrEmpty (slackChannelId))
                configuration[Consts.ENV_NAME_SLACK_CHANNEL_ID] = slackChannelId;

            return configuration;
        }

        private static AlertManagerConfigs MakeCustomConfigs(IConfiguration configuration)
        {
            AlertManagerConfigs configs = new AlertManagerConfigs(
                configuration[CommonConsts.ENV_NAME_ES_URL],
                configuration[CommonConsts.ENV_NAME_ES_USER],
                configuration[CommonConsts.ENV_NAME_ES_PASS],
                configuration[Consts.ENV_NAME_PROM_URL],
                configuration[Consts.ENV_NAME_PROM_JOB],
                configuration[Consts.ENV_NAME_SLACK_APP_KEY],
                configuration[Consts.ENV_NAME_SLACK_CHANNEL_ID]
            );
            return configs;
        }

        private static Serilog.LoggerConfiguration ConfigureNPLogger(AlertManagerConfigs configs)
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

        private static bool CheckApiConnection(AlertManagerConfigs configs)
        {
            bool isTargetValid = false;
            // Connect to the Prometheus
            if (!string.IsNullOrEmpty(configs.PromUrl) && !string.IsNullOrEmpty(configs.PromJob))
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                    using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, $"{configs.PromUrl}api/v1/query"))
                    {
                        var bodyMsg = new List<KeyValuePair<string, string>>();
                        bodyMsg.Add(new("query", "up"));
                        var content = new FormUrlEncodedContent(bodyMsg);
                        reqMsg.Content = content;
                        var response = client.Send(reqMsg, CancellationToken.None);
                        if (!response.IsSuccessStatusCode)
                        {
                            Log.Logger.Error("Failed to connect to the Prometheus server.");
                            return false;
                        }
                    }
                }
                isTargetValid = true;
            }

            if(!isTargetValid)
            {
                Log.Logger.Error("AlertManager is not connected to any server.");
                return false;
            }
            
            // Connect to the Slack
            using(var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Get, $"https://slack.com/api/conversations.info?channel={configs.SlackChannelId}"))
                {
                    reqMsg.Headers.Add("Authorization", $"Bearer {configs.SlackAppToken}");
                    var response = client.Send(reqMsg, CancellationToken.None);
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Logger.Error("Failed to connect to the Slack api.");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}