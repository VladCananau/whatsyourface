// <copyright file="MoqSetupHelpers.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Microsoft.Rest;
    using Moq;
    using Moq.Language.Flow;

    public static class MoqSetupHelpers
    {
        public static ISetup<IFaceOperations, Task<HttpOperationResponse<IList<DetectedFace>>>> SetupDefaultDetectWithStreamWithHttpMessagesAsync(this Mock<IFaceOperations> mock)
        {
            return mock.Setup(m => m.DetectWithStreamWithHttpMessagesAsync(
                    It.IsAny<Stream>(), // image
                    It.IsAny<bool?>(), // returnFaceId
                    It.IsAny<bool?>(), // returnFaceLandmarks
                    It.IsAny<IList<FaceAttributeType>>(),
                    It.IsAny<Dictionary<string, List<string>>>(), // customHeaders
                    It.IsAny<System.Threading.CancellationToken>()));
        }
    }
}
