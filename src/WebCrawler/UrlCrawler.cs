using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class UrlCrawler : IUrlCrawler
    {
        private readonly IDocumentLoader _documentLoader;

        public UrlCrawler(IDocumentLoader documentLoader)
        {
            _documentLoader = documentLoader ?? throw new ArgumentNullException(nameof(documentLoader));
        }
        
        public async Task<(bool success, IEnumerable<UrlResult> urls)> CrawlForUrls(string address)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                Console.WriteLine($"Invalid Url cannot be crawled: {address}");
                return Failed();
            }

            var document = await _documentLoader.GetDocument(uri);

            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Html Document could not be loaded for {address}");
                return Failed();
            }
            
            return (true, document.GetDomainUrls(url => url == uri.Host));
        }

        private (bool, IEnumerable<UrlResult>) Failed() => (false, Enumerable.Empty<UrlResult>());
    }
}