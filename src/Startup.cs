using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace basic_site
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
            services.AddRazorPages();

            services.AddSingleton<ITelemetryInitializer, MyTelemetryInitializer>();

            var aiOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            //aiOptions.EnableAdaptiveSampling = false;
            services.AddApplicationInsightsTelemetry(aiOptions);
            services.AddApplicationInsightsTelemetryProcessor<SynthenicSourceFilter>();
            services.AddApplicationInsightsTelemetryProcessor<FastDependencyCallFilter>();
            services.AddApplicationInsightsTelemetryProcessor<FailedAuthenticationFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //custom extension method
            var telemetryConfiguration = app.ApplicationServices.GetService<TelemetryConfiguration>();

            var builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
        
            // Using adaptive sampling
            builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5);

            // Alternately, the following configures adaptive sampling with 5 items per second, and also excludes DependencyTelemetry from being subject to sampling.
            // builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond:5, excludedTypes: "Dependency");

            // For older versions of the Application Insights SDK, use the following line instead:
            // var builder = configuration.TelemetryProcessorChainBuilder;

            // Using fixed rate sampling
            //double fixedSamplingPercentage = 5;
            //builder.UseSampling(fixedSamplingPercentage);


            builder.Use((next) => new SuccessfulDependencyFilter(next));
            
            builder.Build();



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        public void ConfigureFixedAppInsightsSampling(TelemetryConfiguration configuration, double fixedSamplingPercentage)
        {
            var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            // For older versions of the Application Insights SDK, use the following line instead:
            // var builder = configuration.TelemetryProcessorChainBuilder;

            // Using fixed rate sampling
            builder.UseSampling(fixedSamplingPercentage);
            builder.Build();

            // ...
        }

        public void ConfigureAdaptiveSampling(TelemetryConfiguration configuration)
        {
            var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            // For older versions of the Application Insights SDK, use the following line instead:
            // var builder = configuration.TelemetryProcessorChainBuilder;

            // Using adaptive sampling
            builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5);

            // Alternately, the following configures adaptive sampling with 5 items per second, and also excludes DependencyTelemetry from being subject to sampling.
            // builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond:5, excludedTypes: "Dependency");

            builder.Build();
        }
    }
}
