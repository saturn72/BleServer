﻿using BleServer.Common.Services.Ble;
using BleServer.Common.Services.Notifications;
using BleServer.Modules.Win10BleAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            //services.AddMvc().AddJsonOptions(o =>
            //{
            //    o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //});



            var win10BleAdapter = new Win10BleAdapter();
            win10BleAdapter.Start();
            services.AddSingleton<IBleAdapter>(win10BleAdapter);
            services.AddSingleton<INotifier>(null as INotifier);
            var bluetoothManager = new BleManager(new []{win10BleAdapter}, );
            services.AddSingleton<IBleManager>(bluetoothManager);

            services.AddScoped<IBleService,BleService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Ble Server API", Version = "V1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
            app.UseMvc();
        }
    }
}
