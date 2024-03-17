using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace OmokAPIServer
{
    public class Program
    {
        // ���� �Ʒ� �ּҷ� ���� �� �ܺο��� ������ �ȵǸ� �ּҿ� * �� ����Ѵ�.
        // http://*:11500
        private static string ServerAddress = "http://localhost:11500";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddZLoggerConsole(options =>
                    {
                        options.EnableStructuredLogging = true;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder => { 
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(ServerAddress);
                });
    }
}