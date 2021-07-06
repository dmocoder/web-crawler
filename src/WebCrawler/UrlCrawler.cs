using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace WebCrawler
{
    public class UrlCrawler : IUrlCrawler
    {
        private readonly IDocumentLoader _documentLoader;

        public UrlCrawler(IDocumentLoader documentLoader)
        {
            _documentLoader = documentLoader ?? throw new ArgumentNullException(nameof(documentLoader));
        }
        
        public async Task<(bool success, IEnumerable<string> urls)> CrawlForUrls(string address)
        {
            //validate instruction
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                Console.WriteLine($"Invalid Url cannot be crawled: {address}");
                return Failed();
            }

            var document = await _documentLoader.GetDocument(uri);

            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Html Document could not be loaded for {address}");
            }
                
            return (true, document.GetDomainUrls(url => url == uri.Host));
        }

        private (bool, IEnumerable<string>) Failed() => (false, Enumerable.Empty<string>());
    }
    
    public static class UrlCrawlerExtensions
    {
        public static IEnumerable<string> GetDomainUrls(this IDocument document, Predicate<string> matchQuery)
        {
            var discoveredUrls = new List<string>();

            foreach (var linkElement in document.Links)
            {
                if (linkElement is IHtmlAnchorElement link
                    && matchQuery(link.HostName))
                {
                    discoveredUrls.Add(link.Href);
                }
            }
            
            return discoveredUrls;
        }
    }
}