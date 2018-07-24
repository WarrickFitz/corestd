using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text.RegularExpressions;

//https://bytefish.de/blog/realtime_charts_signalr_chartjs/
namespace EOSNewYork.RealtimeLogPublisher
{
    public class Startup
    {

        private static bool IsOriginAllowed(string host)
        {
            var corsOriginAllowed = new[] { "localhost" };

            return corsOriginAllowed.Any(origin =>
                Regex.IsMatch(host, $@"^http(s)?://.*{origin}(:[0-9]+)?$", RegexOptions.IgnoreCase));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddCors(o =>
            {
                o.AddPolicy("Everything", p =>
                {
                    p.AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(IsOriginAllowed)
                        .AllowCredentials();
                        //.WithOrigins(corsOriginAllowed);
                });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer();
            app.UseCors("Everything");

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });
        }
    }
}