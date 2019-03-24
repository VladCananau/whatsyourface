// <copyright file="UploadImagesToFaceListCmd.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using Dawn;
    using WhatsYourFace.Core;
    using WhatsYourFace.DataSet;
    using WhatsYourFace.Models;

    [Verb("uploadimages", HelpText = "Uploads images from the file system to a facelist.")]
    public class UploadImagesToFaceListCmd : CmdBase<IUploadImagesToFaceListArgs>, IUploadImagesToFaceListArgs
    {
        private readonly IFaceUploader faceUploader;
        private readonly FaceClientSettings settings;

        public UploadImagesToFaceListCmd()
        {
        }

        public UploadImagesToFaceListCmd(
            IFaceUploader faceUploader,
            FaceClientSettings settings,
            IFileSystem fileSystem,
            IConsole console)
            : base(console, fileSystem)
        {
            Guard.Argument(faceUploader, nameof(faceUploader)).NotNull();
            this.faceUploader = faceUploader;
            this.settings = settings;
        }

        public string NameList { get; set; }

        public string Name { get; set; }

        public string ImagesFolder { get; set; }

        public int Count { get; set; }

        public int Skip { get; set; }

        public FaceGender Gender { get; set; }

        public string CountryCode { get; set; }

        public string FaceListId { get; set; }

        public string Output { get; set; }

        public override async Task ExecuteAsync()
        {
            await this.ExecuteAsync(this as IUploadImagesToFaceListArgs);
        }

        public override async Task ExecuteAsync(IUploadImagesToFaceListArgs args)
        {
            if (args.Name != null)
            {
                await this.UploadImagesForOneName(args, args.Name);
            }
            else
            {
                await this.UploadImagesForNameList(args);
            }
        }

        private async Task UploadImagesForNameList(IUploadImagesToFaceListArgs args)
        {
            if (this.settings.SubscriptionTier == "Free")
            {
                Guard.Argument(args.Count, nameof(args.Count)).InRange(1, 20);
            }

            IEnumerable<string> names =
                from csvLine in this.FileSystem.File.ReadLines(args.NameList).Skip(1)
                select csvLine.Split(',')[1];

            int uploadedCount = 0;
            foreach (string name in names)
            {
                uploadedCount += await this.UploadImagesForOneName(args, name);

                if (this.settings.SubscriptionTier == "Free" && uploadedCount >= 20)
                {
                    this.Console.WriteVerbose("Waiting 60s because of Cognitive Services free tier rate limiting");
                    await Task.Delay(60000); // free tier rate limiting
                    uploadedCount = 0;
                }
            }
        }

        private async Task<int> UploadImagesForOneName(IUploadImagesToFaceListArgs args, string name)
        {
            string directory = this.FileSystem.Path.Combine(args.ImagesFolder, name);
            if (this.FileSystem.Directory.Exists(directory))
            {
                this.Console.WriteInformation($"Uploading photos from '{directory}'");

                string userData = $"{args.CountryCode},{args.Gender},{name}";

                IList<Guid> persistedIds = await this.faceUploader.UploadPhotosToFaceList(
                    directory,
                    args.Count,
                    args.Skip,
                    args.FaceListId,
                    userData);

                this.FileSystem.File.AppendAllLines(
                    args.Output,
                    persistedIds.Select(id => $"{id.ToString()},{userData}"));

                return persistedIds.Count;
            }
            else
            {
                this.Console.WriteWarning($"Directory does not exist '{directory}'");
                return 0;
            }
        }
    }
}
