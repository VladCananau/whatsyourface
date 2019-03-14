// <copyright file="IImageDownloader.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System.Threading.Tasks;

    public interface IImageDownloader
    {
        Task DownloadPhotos(PhotoCategory category, string directory, int count, int skip, int maxParallelDownloads, bool overwrite);
    }
}