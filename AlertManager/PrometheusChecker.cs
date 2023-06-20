using Lib.Common;
using Lib.Common.Exceptions;
using Serilog;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AlertManager
{
    public class PrometheusChecker : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly string _promUrl;
        private readonly string _promJob;
        private readonly string _slackAppToken;
        private readonly string _slackChannelId;

        public PrometheusChecker(AlertManagerConfigs configs)
        {
            _logger = Log.Logger.ForContext<PrometheusChecker>();
            _promUrl = configs.PromUrl;
            _promJob = configs.PromJob;
            _slackAppToken = configs.SlackAppToken;
            _slackChannelId = configs.SlackChannelId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                List<Activetarget> unactiveTargets = HealthCheck();
                foreach (Activetarget target in unactiveTargets)
                    SendAlert(target);
                await Task.Delay(3 * 60 * 1000, stoppingToken);
            }
        }

        private List<Activetarget> HealthCheck()
        {
            List<Activetarget> unactiveTargets = new List<Activetarget>();
            using(var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                using(var reqMsg = new HttpRequestMessage(HttpMethod.Get, $"{_promUrl}api/v1/targets?state=any"))
                {
                    var response = client.Send(reqMsg, CancellationToken.None);
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Logger.Error("Failed to check the status of the Prometheus server.");
                        return unactiveTargets;
                    }
                    else
                    {
                        PrometheusTargetsResponse promRes = JsonSerializer.Deserialize<PrometheusTargetsResponse>(response.Content.ReadAsStringAsync().Result);
                        if(promRes == null) 
                        {
                            _logger.Error("Failed to load PrometheusTargetsResponse.");
                        }
                        else if(!promRes.status.Equals("success"))
                        {
                            _logger.Error("Failed to get PrometheusTargetsResponse.");
                        }
                        else
                        { 
                            foreach (var target in promRes.data.activeTargets)
                            {
                                if (!target.health.Equals("up"))
                                {
                                    unactiveTargets.Add(target);
                                }
                            }
                        }
                    }
                }
            }
            return unactiveTargets;
        }

        private void SendAlert(Activetarget unactiveTarget)
        {
            using (var sender = new AlertSender(_slackAppToken, _slackChannelId))
            {
                try
                {
                    bool isSent = sender.Send(_promUrl, unactiveTarget);
                }
                catch(MissingRequiredFieldException ex) 
                {

                }
            }
        }
    }
}