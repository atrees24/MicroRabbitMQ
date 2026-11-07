using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using MicroRabbit.Banking.Application;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Upgrade.CommandHandlers;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Repository;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace MicroRabbit.Infra.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(TransferCommandHandler).Assembly));

            // Domain Bus
            services.AddSingleton<IEventBus>(sp =>
            {
                var mediator = sp.GetRequiredService<IMediator>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var options = sp.GetRequiredService<IOptions<RabbitMQSettings>>();

                return new RabbitMQBus(mediator, scopeFactory, options);
            });

            // Application Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransferService, TransferService>();

            // Command Handlers
            services.AddScoped<IRequestHandler<CreateTransferCommand, bool>,
                TransferCommandHandler>();

            // Data
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransferRepository, TransferRepository>();
        }
    }
}
