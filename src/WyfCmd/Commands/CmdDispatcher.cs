// <copyright file="CmdDispatcher.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using Microsoft.Extensions.DependencyInjection;
    using PowerArgs;
    using WhatsYourFace.Models;
    using WhatsYourFace.Wyfcmd.Commands;

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class CmdDispatcher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConsole console;

        public CmdDispatcher(IServiceProvider serviceProvider)
        {
            Guard.Argument(serviceProvider, nameof(serviceProvider)).NotNull();
            this.serviceProvider = serviceProvider;
            this.console = serviceProvider.GetRequiredService<IConsole>();
        }

        [ArgActionMethod]
        [ArgDescription("Create a new face list in Cognitive Services.")]
        public async Task CreateFaceList(CreateFaceListCmd commandArgs)
        {
            var cmd = this.serviceProvider.GetRequiredService<CreateFaceListCmd>();
            await cmd.ExecuteAsync(commandArgs);
        }

        [ArgActionMethod]
        [ArgDescription("Downloads images from the internet to a local folder.")]
        public async Task DownloadImages(DownloadImagesCmd commandArgs)
        {
            var cmd = this.serviceProvider.GetRequiredService<DownloadImagesCmd>();
            await cmd.ExecuteAsync(commandArgs);
        }

        [ArgActionMethod]
        [ArgDescription("Matches a photo of your face to the names that it most looks like.")]
        public async Task WhatsYourFace(MatchFaceToNamesCmd commandArgs)
        {
            var cmd = this.serviceProvider.GetRequiredService<MatchFaceToNamesCmd>();
            FaceToNameMatchResult result = await cmd.ExecuteAsync(commandArgs);
            this.console.WriteObject(result);
        }

        [ArgActionMethod]
        [ArgDescription("Removes images that contain faces of a different gender than expected.")]
        public async Task RemoveMismatches(RemoveGenderMismatchesCmd commandArgs)
        {
            var cmd = this.serviceProvider.GetRequiredService<RemoveGenderMismatchesCmd>();
            await cmd.ExecuteAsync(commandArgs);
        }

        [ArgActionMethod]
        [ArgDescription("Uploads images from the file system to a facelist.")]
        public async Task UploadImages(UploadImagesToFaceListCmd commandArgs)
        {
            var cmd = this.serviceProvider.GetRequiredService<UploadImagesToFaceListCmd>();
            await cmd.ExecuteAsync(commandArgs);
        }
    }
}
