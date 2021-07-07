namespace WebCrawler
{
    public class UrlResult
    {
        public string Url { get; }
        public bool IsDomain { get; }

        public UrlResult(string url, bool isDomain)
        {
            Url = url;
            IsDomain = isDomain;
        }
    }
}