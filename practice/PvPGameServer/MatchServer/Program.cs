using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

using System.IO;
using System.Threading.Tasks;


namespace MatchServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    var logConfig = hostContext.Configuration.GetSection("LogConfig");
                    logging.ClearProviders();

                    logging.SetMinimumLevel(logConfig["Level"].ToEnum<LogLevel>());

                    // Add Console Logging.
                    logging.AddZLoggerConsole();

                    //logging.AddZLoggerFile("fileName.log");
                    //logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);

                    // Enable Structured Logging
                    /*logging.AddZLoggerConsole(options =>
                    {
                        options.EnableStructuredLogging = true;
                    });*/
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                    services.AddHostedService<MainServer>();
                })
                .Build();

            await host.RunAsync();
        }
    }
     
}
