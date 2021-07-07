using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebCrawler
{
    public interface IUrlCrawler
    {
        Task<(bool success, IEnumerable<UrlResult> urls)> CrawlForUrls(string address);
    }
}