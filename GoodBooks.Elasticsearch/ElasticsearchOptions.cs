namespace GoodBooks.Elasticsearch
{
    public class ElasticsearchOptions
    {
        public const string Section = "elasticsearch";

        public string Url { get; set; }
        public string Index { get; set; }
    }
}