using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager
{

    public class PrometheusTargetsResponse
    {
        public string status { get; set; }
        public TargetsData data { get; set; }
    }

    public class TargetsData
    {
        public Activetarget[] activeTargets { get; set; }
        public object[] droppedTargets { get; set; }
    }

    public class Activetarget
    {
        public Discoveredlabels discoveredLabels { get; set; }
        public Labels labels { get; set; }
        public string scrapePool { get; set; }
        public string scrapeUrl { get; set; }
        public string globalUrl { get; set; }
        public string lastError { get; set; }
        public DateTime lastScrape { get; set; }
        public float lastScrapeDuration { get; set; }
        public string health { get; set; }
        public string scrapeInterval { get; set; }
        public string scrapeTimeout { get; set; }
    }

    public class Discoveredlabels
    {
        public string __address__ { get; set; }
        public string __metrics_path__ { get; set; }
        public string __scheme__ { get; set; }
        public string __scrape_interval__ { get; set; }
        public string __scrape_timeout__ { get; set; }
        public string job { get; set; }
    }

    public class Labels
    {
        public string instance { get; set; }
        public string job { get; set; }
    }

}
