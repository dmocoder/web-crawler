using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Moq;
using Shouldly;
using Xunit;

namespace WebCrawler.Tests
{
    public class UrlCrawlerShould
    {
        private UrlCrawler _urlCrawler;
        private Mock<IDocumentLoader> _documentLoader;
        
        public UrlCrawlerShould()
        {
            _documentLoader = new Mock<IDocumentLoader>();
            _urlCrawler = new UrlCrawler(_documentLoader.Object);
        }
        
        [Fact]
        public async Task ReturnFailed_IfInvalidUrlSupplied()
        {
            var result = await _urlCrawler.CrawlForUrls("dan.fakeaddress");

            result.success.ShouldBeFalse();
        }

        [Fact]
        public async Task ReturnFailed_IfPageReturns429()
        {
            var badDocument = await CreateDocumentStub("", 429);
            _documentLoader.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(badDocument);

            var result = await _urlCrawler.CrawlForUrls("http://localhost:5000");
            result.success.ShouldBeFalse();
        }
        
        [Fact]
        public async Task ReturnFailed_IfPageReturnsRedirect()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var badDocument = await context.OpenAsync(res => res.Content("").Status(301));

            _documentLoader.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(badDocument);
            var result = await _urlCrawler.CrawlForUrls("http://localhost:5011");
            result.success.ShouldBeFalse();
        }
        
        [Fact]
        public async Task ReturnEmpty_IfPageContainsNoLinks()
        {
            var emptyDocument = await CreateDocumentStub("<!DOCTYPE html><html><body><h1>hello</h1><p>paragraph.</p></body></html>");
            _documentLoader.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(emptyDocument);

            var result = await _urlCrawler.CrawlForUrls("http://localhost:5022");
            result.success.ShouldBeTrue();
            result.urls.ShouldBeEmpty();
        }

        [Fact]
        public async Task ReturnLinks_IfPageContainsDomainLinks()
        {
            var linkDocument = await CreateDocumentStub("<!DOCTYPE html><html><body><h1>hello</h1><p>paragraph.</p>" +
                            "<a href=\"http://localhost:5033/here-is-stuff\">stuff</a>" +
                            "<a href=\"http://localhost:5033/here-is-more-stuff\">more stuff</a>" +
                            "</body></html>");
            _documentLoader.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(linkDocument);

            var result = await _urlCrawler.CrawlForUrls("http://localhost:5033");
            result.success.ShouldBeTrue();
            result.urls.Count().ShouldBe(2);
        }
        
        [Fact]
        public async Task ReturnFlaggedDomainLinks_IfPageContainsDomainAndNonDomainLinks()
        {
            var linkDocument = await CreateDocumentStub("<!DOCTYPE html><html><body><h1>hello</h1><p>paragraph.</p>" +
                            "<a href=\"http://localhost:5044/here-is-stuff\">stuff</a>" +
                            "<a href=\"http://foreignhost/here-is-some-foreign-stuff\">foreign stuff</a>" +
                            "</body></html>");
            _documentLoader.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(linkDocument);

            var result = await _urlCrawler.CrawlForUrls("http://localhost:5044");
            result.success.ShouldBeTrue();
            result.urls.Count().ShouldBe(2);
            result.urls.Count(l => l.IsDomain).ShouldBe(1);
        }

        private async Task<IDocument> CreateDocumentStub(string content, int statusCode = 200)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(res => res.Content(content).Status(statusCode));
        }
    }
}
