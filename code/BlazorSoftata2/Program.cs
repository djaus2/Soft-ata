using BlazorSoftata2.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
//using SoftataBasic;
using System.Configuration;
using System.Net.NetworkInformation;
using Blazor.ModalDialog;

namespace BlazorSoftata2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddScoped<AppState>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            //SettingsManager.ClearAllSettings();
            SettingsManager.ReadAllSettings();


            app.Run();
        }

 
    }
}