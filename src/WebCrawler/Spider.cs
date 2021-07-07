using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class Spider
    {
        private readonly int _id;
        private readonly ChannelWriter<string> _discoveredChannel;
        private readonly ChannelReader<string> _instructionChannel;
        private readonly IUrlCrawler _crawler;

        public Spider(int id, ChannelWriter<string> discoveredChannel, ChannelReader<string> instructionChannel, IUrlCrawler crawler)
        {
            _id = id;
            _discoveredChannel = discoveredChannel ?? throw new ArgumentNullException(nameof(discoveredChannel));
            _instructionChannel = instructionChannel ?? throw new ArgumentNullException(nameof(instructionChannel));
            _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                while (await _instructionChannel.WaitToReadAsync(token))
                {
                    var instruction = await _instructionChannel.ReadAsync(token);
                    
                    var crawled = await _crawler.CrawlForUrls(instruction);
                    if (crawled.success)
                    {
                        var links = crawled.urls.ToList();
                        
                        var sb = new StringBuilder();
                        sb.AppendLine($"Visited: {instruction}");
                        sb.AppendLine($"Links Found: {links.Count}");
                        
                        foreach (var urlResult in crawled.urls)
                        {
                            sb.AppendLine($"- {urlResult.Url}");
                            if (urlResult.IsDomain)
                                await _discoveredChannel.WriteAsync(urlResult.Url, token);
                        }

                        Console.WriteLine(sb.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Spider {_id} Cancelled...");
            }
        }
    }
}