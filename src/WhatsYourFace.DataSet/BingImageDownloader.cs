// <copyright file="BingImageDownloader.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Net;
    using System.Threading.Tasks;
    using Dawn;
    using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
    using Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models;
    using Microsoft.Extensions.Logging;

    public class BingImageDownloader : IImageDownloader
    {
        private readonly ILogger<BingImageDownloader> logger;
        private readonly IFileSystem fileSystem;

        public BingImageDownloader(IImageSearchClient imageClient, IFileSystem fileSystem, ILogger<BingImageDownloader> logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
            this.ImageSearchClient = imageClient;
        }

        private BingImageDownloader()
        {
        }

        protected IImageSearchClient ImageSearchClient { get; }

        public async Task DownloadPhotos(PhotoCategory category, string directory, int count, int skip, int maxParallelDownloads, bool overwrite)
        {
            Guard.Argument(count, nameof(count)).InRange(1, 150);
            Guard.Argument(skip, nameof(skip)).NotNegative();
            Guard.Argument(maxParallelDownloads, nameof(maxParallelDownloads)).InRange(1, count);
            Guard.Argument(directory, nameof(directory)).NotNull().NotWhiteSpace();
            Guard.Argument(category, nameof(category))
                .NotNull()
                .Member(c => c.FirstName, n => n.NotNull().NotWhiteSpace());

            this.fileSystem.TestWritePermissionsOrCreateDirectoryIfNotExists(directory);

            Images result = await this.ImageSearchClient.Images.SearchAsync(
                $"{category.FirstName} site:{category.CountryCode}.linkedin.com",
                offset: skip,
                count: count,
                minHeight: 400,
                color: "ColorOnly",
                minWidth: 400,
                imageContent: "Face",
                imageType: "Photo",
                safeSearch: "Strict").ConfigureAwait(false);

            IList<ImageObject> images = result.Value;
            string baseFileName = this.fileSystem.Path.Combine(directory, category.FirstName);

            string session = $"{DateTime.UtcNow.Ticks % 10000:00000}";
            var downloads = new List<Task>(maxParallelDownloads);

            try
            {
                for (int i = 0; i < images.Count; i++)
                {
                    this.logger.LogDebug($"Starting download: {images[i].ThumbnailUrl} with full image at {images[i].HostPageUrl}");

                    WebClient downloadClient = new WebClient();
                    Task task = downloadClient.DownloadFileTaskAsync(
                        new Uri(images[i].ThumbnailUrl), $"{baseFileName}{i:000000}_{session}.{images[i].EncodingFormat}");
                    downloads.Add(task);

                    if (i % maxParallelDownloads == maxParallelDownloads - 1
                        || i == images.Count - 1)
                    {
                        await Task.WhenAll(downloads).ConfigureAwait(false);
                        downloads.Clear();
                    }
                }
            }
            finally
            {
                if (downloads.Count > 0)
                {
                    await Task.WhenAll(downloads).ConfigureAwait(false);
                }
            }
        }
    }
}
