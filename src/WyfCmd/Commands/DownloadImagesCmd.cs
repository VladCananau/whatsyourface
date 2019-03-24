// <copyright file="DownloadImagesCmd.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using Dawn;
    using WhatsYourFace.DataSet;

    [Verb("downloadimages", HelpText = "Downloads images from the internet to a local folder.")]
    public class DownloadImagesCmd : CmdBase<IDownloadImagesArgs>, IDownloadImagesArgs
    {
        private readonly IImageDownloader downloader;

        public DownloadImagesCmd()
        {
        }

        public DownloadImagesCmd(
            IImageDownloader downloader,
            IFileSystem fileSystem,
            IConsole console)
            : base(console, fileSystem)
        {
            Guard.Argument(downloader, nameof(downloader)).NotNull();
            this.downloader = downloader;
        }

        public string NameList { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public int Skip { get; set; }

        public string CountryCode { get; set; }

        public string OutputFolder { get; set; }

        public override async Task ExecuteAsync()
        {
            await this.ExecuteAsync(this as IDownloadImagesArgs);
        }

        public override async Task ExecuteAsync(IDownloadImagesArgs args)
        {
            if (args.Name != null)
            {
                await this.DownloadImagesForOneName(args.Name, args);
            }
            else
            {
                await this.DownloadImagesForNameList(args);
            }
        }

        private async Task DownloadImagesForNameList(IDownloadImagesArgs args)
        {
            IEnumerable<string> names =
                from csvLine in this.FileSystem.File.ReadLines(args.NameList).Skip(1)
                select csvLine.Split(',')[1];

            foreach (string name in names)
            {
                await this.DownloadImagesForOneName(name, args);
            }
        }

        private async Task DownloadImagesForOneName(string name, IDownloadImagesArgs args)
        {
            string directory = this.FileSystem.Path.Combine(args.OutputFolder, name);
            await this.downloader.DownloadPhotos(
                new PhotoCategory { FirstName = name, CountryCode = args.CountryCode },
                directory,
                args.Count,
                args.Skip,
                maxParallelDownloads: 10,
                overwrite: false);
        }
    }
}
