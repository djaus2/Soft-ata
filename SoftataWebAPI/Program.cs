
using Microsoft.OpenApi.Models;
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
                Version = "v5.15",
                Description = "An Arduino API LIKE Firmata for RPI Pico W running Arduino. Includes a .NET package so that you can write your own client in C# to remotely control Pico W devices.",
                Contact = new OpenApiContact()
                {
                    Name = "David Jones",
                    Email = "davidjones@sportronics.com.au",
                }

            };

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", info);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }); 


            ///builder.Services.AddSwaggerGen();
            //// https://github.com/domaindrivendev/Swashbuckle/issues/1083#issuecomment-530471158
            /// https://medium.com/@egwudaujenyuojo/implement-api-documentation-in-net-7-swagger-openapi-and-xml-comments-214caf53eece
            //builder.Services.AddSwaggerGen(c =>
            //{

            //    Set the comments path for the Swagger JSON and UI.

            //   var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            //});

            var app = builder.Build();

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(u =>
                {
                    u.RouteTemplate = "swagger/{documentName}/swagger.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "swagger";
                    c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Your API Title or Version");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
