using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LocalWebServerTest;

public class LocalServerStartup 
{
    public LocalServerStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddControllers();
        services.AddSignalR();
        ////share same instance
        //foreach (var service in App.Services)
        //{
        //    services.AddSingleton(service.ServiceType, App.ServiceProvider.GetRequiredService(service.ServiceType));
        //}

    }


    public void Configure(IApplicationBuilder app)
    {
        
        app.UseRouting();                        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<BingoAppLocalSignalRHub>("/bingoHub");
            endpoints.MapGet("/", async context =>
            {
                var html = System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "bingoRoom.html"));
                await context.Response.WriteAsync(html);
            });
        });

    }

}