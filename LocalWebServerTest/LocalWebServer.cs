using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Threading;

namespace LocalWebServerTest
{
    public class LocalWebServer
    {

        public static string Location => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public static LocalWebServer Current { get; private set; }
        public LocalWebServer()
        {
            Current = this;
            serverThread = new System.Threading.Thread(RunServer);
        }

        private Thread serverThread = default!;

        public void Start()
        {

            serverThread.Start();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            serverThread.Join();
        }

        void RunServer()
        {
            var portNumber = 5050;
            var server = WebHost
           .CreateDefaultBuilder()
           .UseKestrel(x =>
           {
               if (true)
               {
                   x.ListenAnyIP(portNumber);
               }
               x.ListenLocalhost(portNumber);
           })
            .UseStartup<LocalServerStartup>()
            .Build();


            //var builder = new WebHostBuilder();
            //var app = builder.UseKestrel(options =>
            //{
            //    options.ListenLocalhost(5050);
            //})
            //.UseUrls("http://localhost:5050")
            //.ConfigureServices(services =>
            //{
            //    services.AddRouting(options =>
            //    {

            //    });                

            //    //services.AddSignalR();
            //})
            //.Configure(_app =>
            //{



            //    //_app.UseRouter(router =>
            //    //{
            //    //    router.MapGet("/", async context =>
            //    //    {
            //    //        var html = System.IO.File.ReadAllText(System.IO.Path.Combine(Location, "bingoRoom.html"));
            //    //        await context.Response.WriteAsync(html);
            //    //    });                   
            //    //});

            //    //app.UseSignalR(routes =>
            //    //{
            //    //    routes.MapHub<BingoAppLocalSignalRHub>("/bingoHub");
            //    //});

            //})
            //.Build();            
            //app.Run();
            server.RunAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
        }
    }
}
