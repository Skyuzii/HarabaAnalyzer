using System;
using System.Threading.Tasks;
using HarabaAnalyzer.Common;

namespace HarabaAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var worker = IoC.Resolve<Worker>();
            worker.Start(args[0].TrimEnd('\\'));
            
            Console.ReadLine();
        }
    }
}
