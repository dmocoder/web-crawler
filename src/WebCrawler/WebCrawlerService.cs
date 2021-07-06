using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class WebCrawlerService
    {
        private readonly Channel<string> _instructionChannel;
        private readonly Channel<string> _discoveredChannel;
        private readonly HashSet<string> _discovered;
        private readonly List<Spider> _spiders;

        public WebCrawlerService(Channel<string> instructionChannel, Channel<string> discoveredChannel, int spiderCount = 1)
        {
            _instructionChannel = instructionChannel ?? throw new ArgumentNullException(nameof(instructionChannel));
            _discoveredChannel = discoveredChannel ?? throw new ArgumentNullException(nameof(discoveredChannel));
            _discovered = new HashSet<string>();
            _spiders = new List<Spider>();

            for (int i = 0; i < spiderCount; i++)
                _spiders.Add(new Spider(discoveredChannel.Writer, instructionChannel.Reader, new UrlCrawler(new DocumentLoader())));
        }

        public async Task Run(string url, CancellationToken token)
        {
            foreach (var spider in _spiders)
                Task.Run(() => spider.Run(token), token);
            
            _discoveredChannel.Writer.TryWrite(url);

            await Task.Run(() => ListenAndDispatch(token), token);
        }

        private async Task ListenAndDispatch(CancellationToken token)
        {
            Console.WriteLine("Starting Dispatcher...");
            //handle early cancellation 
            try
            {
                while (await _discoveredChannel.Reader.WaitToReadAsync(token))
                {
                    if (_discoveredChannel.Reader.TryRead(out var discoveredItem)
                        && !_discovered.Contains(discoveredItem))
                    {
                        Console.WriteLine($"discovered: {discoveredItem}");
                        
                        _discovered.Add(discoveredItem);
                        await _instructionChannel.Writer.WriteAsync(discoveredItem, token);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Task Cancelled...");
            }
        }
    }
}