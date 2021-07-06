using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;

namespace WebCrawler
{
    public interface IUrlCrawler
    {
        Task<(bool success, IEnumerable<string> urls)> CrawlForUrls(string address);
    }
}