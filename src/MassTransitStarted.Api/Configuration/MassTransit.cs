using MassTransit;
using MassTransit.Definition;
using MassTransitStarted.Api.Providers;
using MassTransitStarted.Components.Consumers;
using MassTransitStarted.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Security.Authentication;

namespace MassTransitStarted.Api.Configuration
{
    public static class MassTransit
    {

        public static IServiceCollection ConfigureMassTransit(this IServiceCollection services)
        {
            services.AddMassTransit((configuration) =>
            {
                // Set consumers and permission to send request
                //configuration.AddConsumer<SubmitOrderConsumer>();
                configuration.AddRequestClient<SubmitOrder>(
                    new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));

                // Config Rabbit
                configuration.UsingRabbitMq((context, configurationHost) =>
                {
                    var rabbitSettings = context.GetRequiredService<IOptions<RabbitSettings>>()?.Value;

                    configurationHost.Host(rabbitSettings.Host, rabbitSettings.Port, rabbitSettings.VirtualHost, (configurationRabbit) =>
                    {
                        configurationRabbit.Username(rabbitSettings.Username);
                        configurationRabbit.Password(rabbitSettings.Password);
                        configurationRabbit.UseSsl(s =>
                        {
                            s.Protocol = SslProtocols.Tls12;
                        });

                        //configurationHost.ConfigureEndpoints(context);
                    });
                });
            });

            services.AddMassTransitHostedService();

            return services;
        }
    }
}
