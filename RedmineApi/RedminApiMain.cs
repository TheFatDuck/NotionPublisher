using AspNetCore.Authentication.ApiKey;
using Lib.Common;
using Lib.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RedmineApi.Data;
using RedmineApi.Middleware;
using RedmineApi.Services;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace RedmineApi
{
    public class RedmineApiMain
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configs: Envs -> appsettings.json -> RedmineApiConfigs. 
            LoadEnvs();
            var configuration = LoadConfiguration(builder);
            RedmineApiConfigs redmineApiConfigs = MakeCustomConfigs(configuration);

            // Logger
            var loggerConfiguration = ConfigureNPLogger(redmineApiConfigs.EsUrl, redmineApiConfigs.EsUser, redmineApiConfigs.EsPass);
            Log.Logger = loggerConfiguration.CreateLogger();


            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddSingleton(redmineApiConfigs);
            builder.Services.AddTransient<DBManager>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            //app.UseMiddleware<ApiKeyMiddleware>(); // Custom middleware for api key auth.

            // Add swagger if degug.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            if(CheckDBConnection(redmineApiConfigs))
                app.Run();
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

            string pgHost = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PG_HOST);
            string pgPort = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PG_PORT);
            string pgUser = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PG_USER);
            string pgPass = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PG_PASS);
            string pgClean = Environment.GetEnvironmentVariable(Consts.ENV_NAME_PG_CLEAN);
            string allowedHosts = Environment.GetEnvironmentVariable(Consts.ENV_NAME_ALLOWED_HOSTS);
            if (string.IsNullOrWhiteSpace(pgHost))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PG_HOST, "host.docker.internal");
            if (string.IsNullOrWhiteSpace(pgPort))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PG_PORT, "5432");
            if (string.IsNullOrWhiteSpace(pgUser))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PG_USER, "admin");
            if (string.IsNullOrWhiteSpace(pgPass))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PG_PASS, "password");
            if (string.IsNullOrWhiteSpace(pgClean))
                Environment.SetEnvironmentVariable(Consts.ENV_NAME_PG_CLEAN, "false");
            if (string.IsNullOrEmpty(allowedHosts))
                allowedHosts = "localhost,127.0.0.1";
            if (!allowedHosts.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                allowedHosts += ",localhost";
            if (!allowedHosts.Contains("127.0.0.1"))
                allowedHosts += ",127.0.0.1";
            Environment.SetEnvironmentVariable(Consts.ENV_NAME_ALLOWED_HOSTS, allowedHosts);
        }

        private static IConfiguration LoadConfiguration(WebApplicationBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
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

            string pgHost = configuration["Postgres:Host"];
            if (!string.IsNullOrWhiteSpace(pgHost))
                configuration[Consts.ENV_NAME_PG_HOST] = pgHost;
            string pgPort = configuration["Postgres:Port"];
            if (!string.IsNullOrWhiteSpace(pgPort))
                configuration[Consts.ENV_NAME_PG_PORT] = pgPort;
            string pgUser = configuration["Postgres:User"];
            if (!string.IsNullOrWhiteSpace(pgUser))
                configuration[Consts.ENV_NAME_PG_USER] = pgUser;
            string pgPass = configuration["Postgres:Pass"];
            if (!string.IsNullOrWhiteSpace(pgPass))
                configuration[Consts.ENV_NAME_PG_PASS] = pgPass;
            string pgClean = configuration["Postgres:CleanDb"];
            if (!string.IsNullOrWhiteSpace(pgClean))
                configuration[Consts.ENV_NAME_PG_CLEAN] = pgClean;

            string allowedHosts = configuration["Auth:AllowedHosts"];
            if (!string.IsNullOrWhiteSpace(allowedHosts))
                configuration[Consts.ENV_NAME_ALLOWED_HOSTS] = allowedHosts;

            return configuration;
        }

        private static LoggerConfiguration ConfigureNPLogger(string esUrl, string esUser, string esPass)
        {
            string projectName = Assembly.GetEntryAssembly().GetName().Name;
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .WriteTo.File(new ElasticsearchJsonFormatter(), $"logs/np-{projectName}-.log", rollingInterval: RollingInterval.Day);
            if (!(string.IsNullOrWhiteSpace(esUrl) || string.IsNullOrWhiteSpace(esUser) || string.IsNullOrWhiteSpace(esPass)))
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUrl))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"np-{projectName}-{{0:yyyy.MM.dd}}",
                    CustomFormatter = new ElasticsearchJsonFormatter(),
                    ModifyConnectionSettings = c => c.BasicAuthentication(esUser, esPass)
                });
            }
            return loggerConfiguration;
        }

        private static RedmineApiConfigs MakeCustomConfigs(IConfiguration configuration)
        {
            RedmineApiConfigs configs = new RedmineApiConfigs(
                configuration[CommonConsts.ENV_NAME_ES_URL],
                configuration[CommonConsts.ENV_NAME_ES_USER],
                configuration[CommonConsts.ENV_NAME_ES_PASS],
                configuration[Consts.ENV_NAME_PG_HOST],
                configuration[Consts.ENV_NAME_PG_PORT],
                configuration[Consts.ENV_NAME_PG_USER],
                configuration[Consts.ENV_NAME_PG_PASS],
                configuration[Consts.ENV_NAME_PG_CLEAN],
                configuration[Consts.ENV_NAME_ALLOWED_HOSTS]
            );
            return configs;
        }

        private static bool CheckDBConnection(RedmineApiConfigs configs)
        {
            try
            {
                DBManager dbManager = new DBManager(configs);
            }
            catch (DBInitializeFailException ex)
            {
                Log.Error($"DBInitializeFailException: {ex.Message}");
                return false;
            }
            catch (NpgsqlException ex)
            {
                Log.Error($"NpgsqlException: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Unknown Exception: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
