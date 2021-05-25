using MassTransit;
using MassTransit.Definition;
using MassTransitStarted.Components.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace MassTransitStarted.Services
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder().ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", true);
                config.AddEnvironmentVariables();

                if (args != null)
                    config.AddCommandLine(args);
            }).ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(KebabCaseEndpointNameFormatter.Instance);

                services.AddMassTransit((configuration) => 
                {
                    configuration.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                    configuration.AddBus(ConfigureBus);
                });

                services.AddHostedService<MassTransitHostedService>();
            }).ConfigureLogging((hostingContext, logging) => 
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IBusRegistrationContext provider)
        {
            return Bus.Factory.CreateUsingRabbitMq((configuration) =>
            {
                configuration.Host("prawn.rmq.cloudamqp.com", 5671, "pbwcmujv", (host) =>
                {
                    host.Username("pbwcmujv");
                    host.Password("efvURO3zDa4kI4nqsJOEQu8x8mBvd8Rb");
                    host.UseSsl(s =>
                    {
                        s.Protocol = SslProtocols.Tls12;
                    });
                });

                configuration.ConfigureEndpoints(provider);
            });
        }
    }
}
