
using Microsoft.OpenApi.Models;
using NetCore2BlocklyNew;
using System.Reflection;

namespace SoftataWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //Swagger Documentation Section
            var info = new OpenApiInfo()
            {
                Title = "SoftataController",
                Version = "v6.00",
                Description = "An Arduino API LIKE Firmata for RPI Pico W running Arduino. Includes a .NET package so that you can write your own client in C# to remotely control Pico W devices.",
                Contact = new OpenApiContact()
                {
                    Name = "David Jones",
                    Email = "davidjones@sportronics.com.au",
                }

            };
            //builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", info);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //app.UseSwagger();
            //app.UseSwaggerUI();
            //}

            app.UseSwagger(u =>
            {
                u.RouteTemplate = "swagger/{documentName}/swagger.json";
            });


            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Softata");
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseBlocklyUI(app.Environment);
            app.UseBlocklyAutomation();

            app.Run();
        }
    }
}
