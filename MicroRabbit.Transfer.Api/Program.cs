
using MicroRabbit.Infra.Bus;
using MicroRabbit.Infra.IoC;
using MicroRabbit.Transfer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroRabbit.Transfer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();



            builder.Services.Configure<RabbitMQSettings>(
                builder.Configuration.GetSection("RabbitMQ"));


            // Register Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Register Dependency Injection
            DependencyContainer.RegisterServices(builder.Services);

            // Register Database Context
            builder.Services.AddDbContext<TransferDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("TransferDbConnection")
                ));

            var app = builder.Build();

            // Configure Swagger UI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
