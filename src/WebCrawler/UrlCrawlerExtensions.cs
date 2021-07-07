using System;
using System.Collections.Generic;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace WebCrawler
{
    public static class UrlCrawlerExtensions
    {
        public static IEnumerable<UrlResult> GetDomainUrls(this IDocument document, Predicate<string> matchQuery)
        {
            var discoveredUrls = new List<UrlResult>();

            foreach (var linkElement in document.Links)
            {
                if (linkElement is IHtmlAnchorElement link)
                    discoveredUrls.Add(new UrlResult(link.Href, matchQuery(link.HostName)));
            }
            
            return discoveredUrls;
        }
    }
}