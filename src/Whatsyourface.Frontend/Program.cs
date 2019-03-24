// <copyright file="Program.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.ApplicationInsights;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    // https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger with Application Insights
                    logging.AddApplicationInsights();
                });

        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration
            // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets
            if (!context.HostingEnvironment.IsDevelopment())
            {
                // TODO (vladcananau): bad practice to build the config here
                // https://github.com/aspnet/Docs/issues/11616
                // Ideally we would want the AddAzureKeyVault to grab a well known
                // KeyVault configuration section;
                var builtConfig = config.Build();

                string keyVaultEndpoint = builtConfig["KeyVault:Endpoint"];

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider.KeyVaultTokenCallback));

                config.AddAzureKeyVault(
                    keyVaultEndpoint,
                    keyVaultClient,
                    new DefaultKeyVaultSecretManager());
            }
        }
    }
}
