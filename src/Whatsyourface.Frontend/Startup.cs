// <copyright file="Startup.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using WhatsYourFace.Core;
    using WhatsYourFace.Frontend.ViewModels;
    using WhatsYourFace.Utilities;

    public class Startup
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<Startup> logger;

        public Startup(IConfiguration configuration, ILoggerFactory factory)
        {
            this.Configuration = configuration;
            this.loggerFactory = factory;
            this.logger = this.loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                this.logger.LogInformation("Startup::ConfigureServices");
                ConfigureCookiePolicy(services);

                services.AddApplicationInsightsTelemetry(this.Configuration);
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
                services.ConfigureLocalization(this.Configuration);
                services.AddCannedExample(this.Configuration);
                services.AddFaceIdLookup(this.Configuration);
                services.AddFaceClient(this.Configuration);
                services.AddFaceMatcher(this.Configuration);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Startup::ConfigureServices");
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                this.logger.LogInformation("Startup::Configure");

                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments
                if (env.IsDevelopment() || env.IsEnvironment("Test1"))
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseCookiePolicy();
                app.UseRequestLocalization(this.BuildLocalizationOptions(app.ApplicationServices));
                app.UseMvc();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Startup::Configure");
                throw;
            }
        }

        private static void ConfigureCookiePolicy(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        private RequestLocalizationOptions BuildLocalizationOptions(IServiceProvider serviceProvider)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization
            LocalizationSettings settings = serviceProvider.GetRequiredService<LocalizationSettings>();

            System.Collections.Generic.List<CultureInfo> supportedCultures =
                (from supportedCulture in settings.SupportedCultures
                 select new CultureInfo(supportedCulture))
                .ToList();

            RequestLocalizationOptions options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(settings.DefaultCulture),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            options.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First().CookieName = settings.CultureCookieName;

            return options;
        }
    }
}
