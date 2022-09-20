using EmailSenderService.EventBusConsumer;
using EmailSenderService.Servises;
using EventBus.Messages.Common;
using MassTransit;
using MassTransit.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmailSenderService
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            AsyncMain(args).GetAwaiter().GetResult();
        }
        public static async Task AsyncMain(string[] args)
        {
            var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configBuilder.AddJsonFile("appsettings.json", optional: true);
                configBuilder.AddJsonFile(
                    $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true);
                configBuilder.AddEnvironmentVariables();
            })

            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                // Read email settings
                services.Configure<EmailConfig>(configuration.GetSection("Email"));

                // Register email service 
                services.AddTransient<IEmailService, EmailService>();
                
                // MassTransit-RabbitMQ Configuration
                services.AddMassTransit(config =>
                {
                    config.AddConsumer<EmailConsumer>();
                    config.UsingRabbitMq((ctx, cfg) =>
                    {

                        cfg.Host(configuration["EventBusSettings:HostAddress"]);
                        cfg.ReceiveEndpoint(EventBusConstants.EmailQueue, c =>
                        {
                            c.ConfigureConsumer<EmailConsumer>(ctx);
                        });
                    });
                });

                services.AddHostedService<Worker>();
                services.AddScoped<EmailConsumer>();
            });

            await hostBuilder.RunConsoleAsync();
        }
    }
}