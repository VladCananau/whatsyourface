// <copyright file="ConfigurationUtilities.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Utilities
{
    using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using WhatsYourFace.Core;
    using WhatsYourFace.DataSet;

    public static class ConfigurationUtilities
    {
        public const string ImageSearchSubscriptionKeyConfigName = "WhatsYourFace.ImageSearchSubscriptionKey";
        public const string ImageSearchEndpointConfigName = "WhatsYourFace.ImageSearchEndpoint";

        private const string FaceClientSettings = "FaceClient";
        private const string FaceMatchSettings = "FaceMatch";
        private const string ImageSearchClientSettings = "ImageSearchClient";
        private const string FaceIdLookupSettings = "FaceIdToNameLookup";

        public static IConfigurationRoot LoadAppSettingsJsonConfiguration(string environmentName)
        {
            return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .Build();
        }

        public static void ConfigureImageSearchClient(IServiceCollection services, IConfiguration config)
        {
            ImageSearchClientSettings settings =
                config.GetSection(ImageSearchClientSettings).Get<ImageSearchClientSettings>();
            services.AddSingleton<ImageSearchClientSettings>(settings);
        }

        public static IServiceCollection AddImageSearchClient(this IServiceCollection services, IConfiguration config)
        {
            ConfigureImageSearchClient(services, config);
            services.AddTransient<IImageSearchClient>(serviceProvider =>
            {
                ImageSearchClientSettings settings = serviceProvider.GetRequiredService<ImageSearchClientSettings>();

                ImageSearchClient searchClient = new ImageSearchClient(
                    new Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials(settings.SubscriptionKey),
                    System.Array.Empty<System.Net.Http.DelegatingHandler>())
                {
                    Endpoint = settings.Endpoint
                };

                return searchClient;
            });

            return services;
        }

        public static void ConfigureFaceClient(IServiceCollection services, IConfiguration config)
        {
            FaceClientSettings settings =
                config.GetSection(FaceClientSettings).Get<FaceClientSettings>();
            services.AddSingleton<FaceClientSettings>(settings);
        }

        public static IServiceCollection AddFaceClient(this IServiceCollection services, IConfiguration config)
        {
            ConfigureFaceClient(services, config);

            services.AddTransient<IFaceClient>(serviceProvider =>
            {
                FaceClientSettings settings = serviceProvider.GetRequiredService<FaceClientSettings>();

                FaceClient faceClient = new FaceClient(
                    new Microsoft.Azure.CognitiveServices.Vision.Face.ApiKeyServiceClientCredentials(settings.SubscriptionKey),
                    System.Array.Empty<System.Net.Http.DelegatingHandler>())
                {
                    Endpoint = settings.Endpoint
                };

                return faceClient;
            });

            return services;
        }

        public static void AddFaceIdLookup(this IServiceCollection services, IConfiguration config)
        {
            FaceIdToNameCsvSourceSettings settings =
                config.GetSection(FaceIdLookupSettings).Get<FaceIdToNameCsvSourceSettings>();
            services.AddSingleton<IFaceIdToNameLookup>(MemoryFaceIdToNameLookup.FromCsvFile(settings));
        }

        public static void ConfigureFaceMatch(this IServiceCollection services, IConfiguration config)
        {
            FaceMatchSettings settings =
                config.GetSection(FaceMatchSettings).Get<FaceMatchSettings>();
            services.AddSingleton<FaceMatchSettings>(settings);
        }

        public static void AddFaceMatcher(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureFaceMatch(config);
            services.AddTransient<IFaceMatcher, FaceMatcher>();
        }

        public static void AddFaceUploader(this IServiceCollection services)
        {
            services.AddTransient<IFaceUploader, FaceUploader>();
        }

        public static void AddImageDownloader(this IServiceCollection services)
        {
            services.AddTransient<IImageDownloader, BingImageDownloader>();
        }

        public static void AddImageSanitizer(this IServiceCollection services)
        {
            services.AddTransient<IImageSanitizer, ImageSanitizer>();
        }
    }
}
