using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Infra.IoC;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace MicroRabbit.Transfer.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // RabbitMQ Configuration
            builder.Services.Configure<RabbitMQSettings>(
                builder.Configuration.GetSection("RabbitMQ"));

            // Transfer Services Only
            builder.Services.AddTransferServices();

            // RabbitMQ Infrastructure
            builder.Services.AddRabbitMQInfrastructure();

            // Transfer Database
            builder.Services.AddDbContext<TransferDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "TransferDbConnection")));

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            await ConfigureEventBus(app);

            app.Run();
        }
        private static async Task ConfigureEventBus(WebApplication app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            await eventBus.Subscribe<TransferCreatedEvent, TransferEventHandler>();
        }
    }
}