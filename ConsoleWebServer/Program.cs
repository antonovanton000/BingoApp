// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

Console.WriteLine("Hello, World!");
var builder = new WebHostBuilder();
var app = builder.UseKestrel(options =>
{
    options.ListenLocalhost(5050);
})
.UseUrls("http://localhost:5050")
.ConfigureServices(services =>
{
    services.AddRouting(options =>
    {

    });
    //services.AddSignalR();
})
.Configure(_app =>
{
    _app.UseRouter(router =>
    {
        router.MapGet("/", async context =>
        {
            var html = System.IO.File.ReadAllText("bingoRoom.html");
            await context.Response.WriteAsync(html);
        });

    });


    //app.UseSignalR(routes =>
    //{
    //    routes.MapHub<BingoAppLocalSignalRHub>("/bingoHub");
    //});

})
.Build();

app.Run();