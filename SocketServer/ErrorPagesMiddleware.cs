namespace SocketServer;

public static class ErrorPagesMiddleware
{
    /// <summary>
    /// This writes the page out into the response using the
    /// status code of the error that was generated (400,500,etc).
    /// </summary>
    /// <param name="app"></param>
    /// <param name="filePath">The error page (based in wwwroot)</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">File path cannot be null</exception>
    /// <exception cref="ArgumentException">File must exist</exception>
    public static IApplicationBuilder UseErrorPages(
        this IApplicationBuilder app,
        string filePath)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        var env = app.ApplicationServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var file = Path.Combine(env.WebRootPath, filePath);

        if (!File.Exists(file))
            throw new ArgumentException("file does not exist", nameof(filePath));

        app.Use(async (context, next) =>
        {
            await next.Invoke();

            var handled = context.Features.Get<ErrorPageFeature>();
            var statusCode = context.Response.StatusCode;
            if (handled == null && statusCode >= 400)
            {
                var page = await File.ReadAllTextAsync(file);
                context.Response.Clear();
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsync(page);

                // make sure we don't get into an infinite loop
                context.Features.Set(new ErrorPageFeature());
            }
        });

        return app;
    }

    private class ErrorPageFeature
    {
    }
}
