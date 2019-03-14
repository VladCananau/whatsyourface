// <copyright file="ConfigurationUtilities.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend
{
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using WhatsYourFace.Frontend.ViewModels;

    public static class FrontendConfigurationUtilities
    {
        public static ICannedExample LoadCannedExample(IConfiguration config)
        {
            IConfigurationSection settings = config.GetSection("CannedExample");
            string json = File.ReadAllText(settings["FilePath"]);
            return CannedExample.Deserialize(json);
        }

        public static void AddCannedExample(this IServiceCollection services, IConfiguration config)
        {
            ICannedExample example = LoadCannedExample(config);
            services.AddSingleton(example);
        }

        public static void ConfigureLocalization(this IServiceCollection services, IConfiguration config)
        {
            LocalizationSettings settings = config.GetSection("Localization").Get<LocalizationSettings>();
            settings.Validate();
            services.AddSingleton(settings);
        }
    }
}
