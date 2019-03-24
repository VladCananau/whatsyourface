// <copyright file="ImageSanitizer.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Microsoft.Extensions.Logging;
    using WhatsYourFace.Core;
    using WhatsYourFace.Models;

    public class ImageSanitizer : IImageSanitizer
    {
        private readonly IFileSystem fileSystem;
        private readonly IFaceClient faceClient;
        private readonly ILogger<ImageSanitizer> logger;

        public ImageSanitizer(IFileSystem fileSystem, IFaceClient faceClient, ILogger<ImageSanitizer> logger)
        {
            this.fileSystem = fileSystem;
            this.faceClient = faceClient;
            this.logger = logger;
        }

        public async Task RemoveGenderMismatches(string imagesDirectory, FaceGender expectedGender, string quarantineDirectory)
        {
            this.fileSystem.TestWritePermissionsOrCreateDirectoryIfNotExists(quarantineDirectory);

            foreach (string imageFile in this.fileSystem.Directory
                .EnumerateFiles(imagesDirectory, "*", SearchOption.AllDirectories))
            {
                try
                {
                    using (Stream stream = this.fileSystem.File.OpenRead(imageFile))
                    {
                        IList<DetectedFace> faces = await this.faceClient.Face.DetectWithStreamAsync(
                            stream,
                            returnFaceId: true,
                            returnFaceLandmarks: false,
                            new[] { FaceAttributeType.Gender, FaceAttributeType.Age });

                        Gender detectedGender = faces[0].FaceAttributes.Gender.Value;

                        if (faces.Count != 1)
                        {
                            this.logger.LogDebug(
                                $"Removing '{imageFile}' because the image contains {faces.Count} faces instead of 1");
                        }
                        else if (detectedGender.IsSameAs(expectedGender))
                        {
                            this.logger.LogDebug(
                                $"Removing '{imageFile}' because the detected gender '{detectedGender}' is not '{expectedGender}'");
                        }
                        else
                        {
                            continue;
                        }

                        string destinationFile = this.fileSystem.Path.Combine(
                            quarantineDirectory,
                            this.fileSystem.Path.GetFileName(imageFile));
                        this.fileSystem.File.Move(imageFile, destinationFile);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"Skipping '{imageFile}' due to error: '{ex.Message}'");
                }
            }
        }
    }
}
