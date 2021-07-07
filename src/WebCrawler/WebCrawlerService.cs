using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace WebCrawler
{
    public class WebCrawlerService
    {
        private readonly Channel<string> _instructionChannel;
        private readonly Channel<string> _discoveredChannel;
        private readonly HashSet<string> _discovered;
        private readonly List<Spider> _spiders;

        private System.Timers.Timer _idleTimer;
        private int _idleTimeTotal;
        
        public WebCrawlerService(Channel<string> instructionChannel, Channel<string> discoveredChannel,
            int spiderCount = 1)
        {
            _instructionChannel = instructionChannel ?? throw new ArgumentNullException(nameof(instructionChannel));
            _discoveredChannel = discoveredChannel ?? throw new ArgumentNullException(nameof(discoveredChannel));
            _discovered = new HashSet<string>();
            _spiders = new List<Spider>();

            for (int i = 0; i < spiderCount; i++)
                _spiders.Add(new Spider(i, discoveredChannel.Writer, instructionChannel.Reader,
                    new UrlCrawler(new DocumentLoader())));
        }

        public async Task Run(string url, CancellationToken token)
        {
            foreach (var spider in _spiders)
                _ = Task.Run(() => spider.Run(token), token);

            Console.WriteLine($"Crawling {url}...");
            var sw = new Stopwatch();
            sw.Start();
            StartIdleTimer();
            
            _discoveredChannel.Writer.TryWrite(url);
            
            await Task.Run(() => ListenAndDispatch(token), token);
            
            sw.Stop();
            Console.WriteLine($"Crawling {url} completed in {sw.Elapsed}");
        }

        private async Task ListenAndDispatch(CancellationToken token)
        {
            try
            {
                while (await _discoveredChannel.Reader.WaitToReadAsync(token) && !token.IsCancellationRequested)
                {
                    if (_discoveredChannel.Reader.TryRead(out var discoveredItem)
                        && !_discovered.Contains(discoveredItem))
                    {
                        _discovered.Add(discoveredItem);
                        await _instructionChannel.Writer.WriteAsync(discoveredItem, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task Cancelled...");
            }
        }

        private void StartIdleTimer()
        {
            _idleTimer = new System.Timers.Timer(1000);
            _idleTimer.Elapsed += IdleTimerOnElapsed;
            _idleTimer.AutoReset = true;
            _idleTimer.Enabled = true;
        }

        private void IdleTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_discoveredChannel.Reader.Count == 0 && _instructionChannel.Reader.Count == 0)
                _idleTimeTotal += 1;
            else 
                _idleTimeTotal = 0;

            if (5 < _idleTimeTotal)
            {
                _discoveredChannel.Writer.TryComplete();
                _instructionChannel.Writer.TryComplete();
            }
        }
    }
}