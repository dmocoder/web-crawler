using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var discoveredChannel = Channel.CreateUnbounded<string>();
            var instructionChannel = Channel.CreateUnbounded<string>();

            var crawler = new WebCrawlerService(instructionChannel, discoveredChannel, 1);

            var tkn = new CancellationTokenSource();
            await Task.Run(() => crawler.Run("https://monzo.com", tkn.Token), tkn.Token);
        }
    }
}
