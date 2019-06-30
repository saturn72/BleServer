using System;
using BleServer.Common.Services.Ble;
using BleServer.Common.Services.Notifications;
using BleServer.Modules.Win10BleAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace BleServer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR(options => options.EnableDetailedErrors = true);
            services.AddSingleton<MessageHub>();
            services.AddSingleton<INotifier, SignalRNotifier>();

            var win10BleAdapter = new Win10BleAdapter();
            win10BleAdapter.Start();
            services.AddSingleton<IBleAdapter>(win10BleAdapter);
            services.AddSingleton<IBleManager>(sr => new BleManager(new[] { win10BleAdapter }, sr.GetService<INotifier>()));

            services.AddScoped<IBleService, BleService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Ble Server API", Version = "V1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitAppServices(app.ApplicationServices);
            
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ble Server API V1");
            });

            app.UseSignalR(route =>
            {
                route.MapHub<MessageHub>("/ws");
            });

            app.UseMvc();
        }

        private void InitAppServices(IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<IBleManager>();
        }
    }
}
