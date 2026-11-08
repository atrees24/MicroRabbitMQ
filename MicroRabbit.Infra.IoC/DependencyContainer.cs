
using MediatR;
using MicroRabbit.Banking.Application;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Upgrade.CommandHandlers;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Repository;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroRabbit.Infra.IoC
{
    public static class DependencyContainer
    {
        // Shared RabbitMQ Infrastructure
        public static IServiceCollection AddRabbitMQInfrastructure(
            this IServiceCollection services)
        {
            services.AddOptions<RabbitMQSettings>()
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.HostName),
                    "RabbitMQ:HostName must be configured.")
                .Validate(
                    settings => settings.Port is > 0 and <= 65535,
                    "RabbitMQ:Port must be between 1 and 65535.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.UserName),
                    "RabbitMQ:UserName is required. Set it with .NET User Secrets.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.Password),
                    "RabbitMQ:Password is required. Set it with .NET User Secrets.")
                .ValidateOnStart();

            services.AddSingleton<IEventBus>(sp =>
            {
                var scopeFactory =
                    sp.GetRequiredService<IServiceScopeFactory>();

                var options =
                    sp.GetRequiredService<IOptions<RabbitMQSettings>>();

                return new RabbitMQBus(scopeFactory, options);
            });

            return services;
        }

        // Banking API Registrations
        public static IServiceCollection AddBankingServices(
            this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(TransferCommandHandler).Assembly));

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAccountRepository, AccountRepository>();

            return services;
        }

        // Transfer API Registrations
        public static IServiceCollection AddTransferServices(
            this IServiceCollection services)
        {
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<ITransferRepository, TransferRepository>();

            services.AddScoped<IEventHandler<TransferCreatedEvent>,
                TransferEventHandler>();

            return services;
        }
    }
}
