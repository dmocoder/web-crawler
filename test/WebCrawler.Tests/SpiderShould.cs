using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace WebCrawler.Tests
{
    public class SpiderShould
    {
        private readonly Spider _spider;
        private readonly Mock<IUrlCrawler> _urlCrawler;

        private readonly Channel<string> _discoveredChannel;
        private readonly Channel<string> _instructionChannel;
        
        public SpiderShould()
        {
            _discoveredChannel = Channel.CreateUnbounded<string>();
            _instructionChannel = Channel.CreateUnbounded<string>();
            _urlCrawler = new Mock<IUrlCrawler>();

            _spider = new Spider(1, _discoveredChannel.Writer, _instructionChannel.Reader, _urlCrawler.Object);
        }

        [Fact]
        public async Task AddUrlsToChannel()
        {
            var instruction = "http://localhost:5089";
            var discoveredUrl = instruction + "/cool-stuff";

            _urlCrawler.Setup(x => x.CrawlForUrls(instruction))
                .ReturnsAsync((true, new[] {new UrlResult(discoveredUrl, true)}));

            var token = new CancellationTokenSource();
            var _ = Task.Run(() => _spider.Run(token.Token));
            
            _instructionChannel.Writer.TryWrite(instruction);
            var discoveredItem = await _discoveredChannel.Reader.ReadAsync();
            discoveredItem.ShouldBe(discoveredUrl);
            token.Cancel();
        }
        
        [Fact]
        public async Task AddUrlsToChannel_OnlyFromDomain()
        {
            var instruction = "http://localhost:5089";
            var discoveredUrl = instruction + "/cool-stuff";
            var nonDomainUrl = "elsewhere" + "/cool-stuff";

            _urlCrawler.Setup(x => x.CrawlForUrls(instruction))
                .ReturnsAsync((true, new[] { new UrlResult(discoveredUrl, true), new UrlResult(nonDomainUrl, false)}));

            var token = new CancellationTokenSource();
            var _ = Task.Run(() => _spider.Run(token.Token));
            
            _instructionChannel.Writer.TryWrite(instruction);

            //arbitrary delay to allow spider to get up and running
            await Task.Delay(2000, token.Token);
            _discoveredChannel.Reader.Count.ShouldBe(1);
            token.Cancel();
        }
    }
}