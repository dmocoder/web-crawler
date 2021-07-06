using System;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace WebCrawler
{
    public class DocumentLoader : IDocumentLoader
    {
        public async Task<IDocument> GetDocument(Uri address)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            
            return await context.OpenAsync(address.ToString());
        }
    }
}