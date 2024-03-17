using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Threading.Tasks;

namespace LobbyServer
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
                    logging.AddZLoggerConsole(options =>
                    {
                        options.EnableStructuredLogging = true;
                    });
                    #region ZLogger 파일 로그
                    //logging.AddZLoggerFile("fileName.log");
                    //logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);

                    // Enable Structured Logging
                    /*logging.AddZLoggerConsole(options =>
                    {
                        options.EnableStructuredLogging = true;
                    });*/
					#endregion
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                    services.AddHostedService<MainServer>();
                })
                .Build();

			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            ServerCommon.LogManager.SetLoggerFactory(loggerFactory, "LobbyServer");
			
            await host.RunAsync();
        }
    }
}
