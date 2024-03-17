using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

using System;

namespace DBServer
{
    class Program
    {
        public static ILogger GlobalLogger;

        static void Main(string[] args)
        {
            GlobalLogger = CreateLogger();

            var serverOption = ParseCommandLine(args);
            
            var serverApp = new DBServer();
            serverApp.Start(serverOption);

            GlobalLogger.LogInformation("Start DBServer !");
            GlobalLogger.LogInformation("Press q to shut down the server");
            

            while (true)
            {
                System.Threading.Thread.Sleep(128);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.KeyChar == 'q')
                {
                    GlobalLogger.LogInformation("Server Stop ~~~");
                    
                    serverApp.Stop();
                    break;
                }
                else
                {
                    GlobalLogger.LogInformation($"Preessed key:{key.KeyChar}");
                }
            }

            GlobalLogger.LogInformation("Server Terminate ~~~");
        }
         

        static ILogger CreateLogger()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(options =>
            {
                //options.AddZLoggerFile("ZLogger.log");
                options.AddZLoggerConsole(options =>
                {
                    var prefixFormat = Cysharp.Text.ZString.PrepareUtf8<LogLevel, DateTime>("[{0}][{1}]");
                    options.PrefixFormatter = (writer, info) => prefixFormat.FormatTo(ref writer, info.LogLevel, info.Timestamp.DateTime.ToLocalTime());
                });
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerProvider>();

            return factory.CreateLogger("DBServer-Main");
        }

        static ServerOption ParseCommandLine(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

            if (result == null)
            {
                GlobalLogger.LogError("Failed Command Line");
                return null;
            }                       

            return result.Value;
        }
    }
}
