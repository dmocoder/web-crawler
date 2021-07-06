using System;
using System.Threading.Tasks;
using AngleSharp.Dom;

namespace WebCrawler
{
    public interface IDocumentLoader
    {        
        Task<IDocument> GetDocument(Uri address);
    }
}