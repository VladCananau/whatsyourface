// <copyright file="FaceUploader.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Microsoft.Extensions.Logging;
    using WhatsYourFace.Models;

    public class FaceUploader : IDisposable, IFaceUploader
    {
        private readonly IFileSystem fileSystem;
        private readonly IFaceClient faceClient;
        private readonly ILogger<FaceUploader> logger;

        public FaceUploader(IFaceClient faceClient, IFileSystem fileSystem, ILogger<FaceUploader> logger)
        {
            this.logger = logger;
            Guard.Argument(faceClient, nameof(faceClient)).NotNull();
            this.faceClient = faceClient;
            this.fileSystem = fileSystem;
        }

        public void Dispose()
        {
            this.faceClient.Dispose();
        }

        public async Task<IList<Guid>> UploadPhotosToFaceList(string directory, int count, int skip, string faceListId, string userData)
        {
            Guard.Argument(count, nameof(count)).NotNegative();
            Guard.Argument(skip, nameof(skip)).NotNegative();
            Guard.Argument(directory, nameof(directory)).NotNull().NotWhiteSpace();
            Guard.Argument(directory, nameof(faceListId))
                .NotNull()
                .NotWhiteSpace()
                .Require(dir => this.fileSystem.Directory.Exists(dir), dir => $"Directory does not exist '{dir}'");

            IEnumerable<string> files = this.fileSystem.Directory.EnumerateFiles(directory).Skip(skip).Take(count);

            List<Guid> faceIds = new List<Guid>(count);
            foreach (string imageFile in files)
            {
                using (Stream stream = this.fileSystem.File.OpenRead(imageFile))
                {
                    PersistedFace persistedFace = await this.faceClient.FaceList.AddFaceFromStreamAsync(
                        faceListId,
                        stream,
                        userData);

                    this.logger.LogDebug($"Added '{imageFile}' to FaceList '{faceListId}' with PersistedFaceId {persistedFace.PersistedFaceId}");
                    faceIds.Add(persistedFace.PersistedFaceId);
                }
            }

            return faceIds;
        }

        private static bool IsSameGender(Gender detectedGender, FaceGender statedGender)
        {
            return (detectedGender == Gender.Male && statedGender == FaceGender.Male)
                || (detectedGender == Gender.Female && statedGender == FaceGender.Female)
                || (detectedGender != Gender.Male && detectedGender != Gender.Female
                    && statedGender != FaceGender.Male && statedGender != FaceGender.Female);
        }
    }
}
