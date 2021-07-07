using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 1 || !Uri.TryCreate(args[0], UriKind.Absolute, out var uri))
            {
                Console.WriteLine("Invalid arguments supplied, please supply a valid url");
                return -1;
            }

            var discoveredChannel = Channel.CreateUnbounded<string>();
            var instructionChannel = Channel.CreateUnbounded<string>();

            var crawler = new WebCrawlerService(instructionChannel, discoveredChannel, 1);

            var tkn = new CancellationTokenSource();
            await Task.Run(() => crawler.Run(uri.ToString(), tkn.Token), tkn.Token);

            return 0;
        }
    }
}