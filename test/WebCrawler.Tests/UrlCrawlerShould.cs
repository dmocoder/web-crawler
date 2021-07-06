using System;
using System.Threading.Tasks;
using AngleSharp;
using Moq;
using Shouldly;
using Xunit;

namespace WebCrawler.Tests
{
    public class UrlCrawlerShould
    {
        [Fact]
        public async Task ReturnFailed_IfInvalidUrlSupplied()
        {
            var urlCrawler = new UrlCrawler(Mock.Of<IDocumentLoader>());

            var result = await urlCrawler.CrawlForUrls("dan.fakeaddress");

            result.success.ShouldBeFalse();
        }

        [Fact]
        public async Task ReturnFailed_IfPageReturns429()
        {
            var loaderMock = new Mock<IDocumentLoader>();
            
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var badDocument = await context.OpenAsync(res => res.Content("").Status(429));

            loaderMock.Setup(x => x.GetDocument(It.IsAny<Uri>())).ReturnsAsync(badDocument);
            
        }
    }
}
