namespace AlertManager
{
    public class PrometheusQueryResponse
    {
        public string status { get; set; }
        public QueryData data { get; set; }
    }

    public class QueryData
    {
        public string resultType { get; set; }
        public QueryResult[] result { get; set; }
    }

    public class QueryResult
    {
        public QueryMetric metric { get; set; }
        public object[] value { get; set; }
    }

    public class QueryMetric
    {
        public string __name__ { get; set; }
        public string instance { get; set; }
        public string job { get; set; }
    }

}
