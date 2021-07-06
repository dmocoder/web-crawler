using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class Spider
    {
        private readonly ChannelWriter<string> _discoveredChannel;
        private readonly ChannelReader<string> _instructionChannel;
        private readonly IUrlCrawler _crawler;

        public Spider(ChannelWriter<string> discoveredChannel, ChannelReader<string> instructionChannel, IUrlCrawler crawler)
        {
            _discoveredChannel = discoveredChannel ?? throw new ArgumentNullException(nameof(discoveredChannel));
            _instructionChannel = instructionChannel ?? throw new ArgumentNullException(nameof(instructionChannel));
            _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
        }

        public async Task Run(CancellationToken token)
        {
            Console.WriteLine("Starting spider...");
            
            try
            {
                while (await _instructionChannel.WaitToReadAsync(token))
                {
                    var instruction = await _instructionChannel.ReadAsync(token);

                    var crawled = await _crawler.CrawlForUrls(instruction);
                    if (crawled.success)
                        foreach (var url in crawled.urls)
                        {
                            await _discoveredChannel.WriteAsync(url, token);
                        }
                }
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine("Spider Cancelled...");
            }
        }
    }
}