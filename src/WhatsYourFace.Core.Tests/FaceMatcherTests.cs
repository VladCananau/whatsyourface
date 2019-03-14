// <copyright file="FaceMatcherTests.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Rest;
    using Moq;
    using Shouldly;
    using WhatsYourFace.Core;
    using WhatsYourFace.Models;
    using Xunit;

    public class FaceMatcherTests
    {
        private MockRepository mockRepository;
        private FaceMatchSettings faceMatchSettings;

        private Mock<IFaceIdToNameLookup> mockFaceIdLookup;
        private Mock<IFaceClient> mockFaceClient;
        private Mock<IFaceOperations> mockFaceOperations;
        private Mock<ILogger<FaceMatcher>> mockLogger;

        public FaceMatcherTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockFaceIdLookup = this.mockRepository.Create<IFaceIdToNameLookup>();

            this.mockFaceOperations = this.mockRepository.Create<IFaceOperations>();
            this.mockFaceClient = this.mockRepository.Create<IFaceClient>();
            this.mockFaceClient.Setup(mock => mock.Face).Returns(this.mockFaceOperations.Object);
            this.mockFaceClient.Setup(mock => mock.Dispose());

            this.faceMatchSettings = 
                new FaceMatchSettings() { FaceListNameFormat = "faces-{countrycode}-{gender}" };

            this.mockLogger = this.mockRepository.Create<ILogger<FaceMatcher>>(MockBehavior.Loose);
        }

        public FaceMatcher CreateFaceMatcher()
        {
            return new FaceMatcher(
                this.mockFaceClient.Object,
                this.mockFaceIdLookup.Object,
                this.faceMatchSettings,
                this.mockLogger.Object);
        }

        [Fact]
        public void Dispose_CallsFaceClientDispose()
        {
            // Arrange
            var unitUnderTest = this.CreateFaceMatcher();

            // Act
            unitUnderTest.Dispose();

            // Assert
            this.mockFaceClient.Verify(mock => mock.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DetectSingleFaceAsync_Success()
        {
            // Arrange
            var unitUnderTest = this.CreateFaceMatcher();
            Stream photo = new MemoryStream();

            DetectedFace expected = new DetectedFace();
            expected.FaceId = Guid.NewGuid();
            expected.FaceAttributes = new FaceAttributes(gender: Gender.Female);
            var response = new Microsoft.Rest.HttpOperationResponse<IList<DetectedFace>>();
            response.Body = new[] { expected };

            this.mockFaceOperations
                .Setup(mock => mock.DetectWithStreamWithHttpMessagesAsync(
                    photo,
                    true, // returnFaceId
                    false, // returnFaceLandmarks
                    It.Is<IList<FaceAttributeType>>(list => list.Count == 1 && list.Contains(FaceAttributeType.Gender)),
                    null, // customeHeaders
                    It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            DetectedFace result = await unitUnderTest.DetectSingleFaceAsync(photo);

            // Assert
            result.ShouldNotBeNull();
            result.FaceId.ShouldBe(expected.FaceId);
            result.FaceAttributes.Gender.ShouldBe(expected.FaceAttributes.Gender);
        }

        [Fact]
        public async Task DetectSingleFaceAsync_Error_ZeroFaces()
        {
            // Arrange
            var unitUnderTest = this.CreateFaceMatcher();

            var response = new HttpOperationResponse<IList<DetectedFace>>();
            response.Body = new DetectedFace[0];

            this.mockFaceOperations
                .SetupDefaultDetectWithStreamWithHttpMessagesAsync()
                .Returns(Task.FromResult(response));

            // Act
            FaceMatchException ex = await Should.ThrowAsync<FaceMatchException>(
                async () => await unitUnderTest.DetectSingleFaceAsync(Stream.Null));
            ex.Message.ShouldNotBeNullOrWhiteSpace();
            ex.ErrorCode.ShouldBe(FaceMatchException.Codes.ZeroFacesInPhoto);
        }

        [Fact]
        public async Task DetectSingleFaceAsync_Error_TwoFaces()
        {
            // Arrange
            var unitUnderTest = this.CreateFaceMatcher();

            var response = new HttpOperationResponse<IList<DetectedFace>>();
            response.Body = new[] { new DetectedFace(), new DetectedFace() };

            this.mockFaceOperations
                .SetupDefaultDetectWithStreamWithHttpMessagesAsync()
                .Returns(Task.FromResult(response));

            // Act
            FaceMatchException ex = await Should.ThrowAsync<FaceMatchException>(
                async () => await unitUnderTest.DetectSingleFaceAsync(Stream.Null));
            ex.Message.ShouldNotBeNullOrWhiteSpace();
            ex.ErrorCode.ShouldBe(FaceMatchException.Codes.MoreThanOneFaceInPhoto);
        }

        [Fact]
        public async Task MatchFaceToNameAsync_Success()
        {
            // Arrange
            var unitUnderTest = this.CreateFaceMatcher();
            Stream photo = new MemoryStream();
            string countryCode = "ro";
            int maxSimilarFaces = 10;

            Guid fakeFaceId = Guid.NewGuid();
            DetectedFace fakeDetectedFace = new DetectedFace();
            fakeDetectedFace.FaceId = fakeFaceId;
            fakeDetectedFace.FaceAttributes = new FaceAttributes(gender: Gender.Female);
            var detectFaceResponse = new HttpOperationResponse<IList<DetectedFace>>();
            detectFaceResponse.Body = new[] { fakeDetectedFace };

            this.mockFaceOperations
                .Setup(mock => mock.DetectWithStreamWithHttpMessagesAsync(
                    photo,
                    true, // returnFaceId
                    false, // returnFaceLandmarks
                    It.Is<IList<FaceAttributeType>>(list => list.Count == 1 && list.Contains(FaceAttributeType.Gender)),
                    null, // customeHeaders
                    It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(detectFaceResponse));

            string fakeFaceListId = "faces-ro-female";
            Guid fakePersistedFaceId1 = Guid.Parse("11110000-1100-1100-1100-111111000000");
            Guid fakePersistedFaceId2 = Guid.Parse("22220000-2200-2200-2200-222222000000");
            var fakeSimilarFaces = new[]
            {
                new SimilarFace() { PersistedFaceId = fakePersistedFaceId1, Confidence = 0.8765 },
                new SimilarFace() { PersistedFaceId = fakePersistedFaceId2, Confidence = 0.4321 },
            };

            var findSimilarResponse = new HttpOperationResponse<IList<SimilarFace>>();
            findSimilarResponse.Body = fakeSimilarFaces;

            this.mockFaceOperations
                .Setup(mock => mock.FindSimilarWithHttpMessagesAsync(
                    fakeFaceId,
                    fakeFaceListId,
                    null, // largeFaceListId
                    null, // faceIds
                    maxSimilarFaces,
                    FindSimilarMatchMode.MatchFace,
                    null, // customheaders
                    It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(findSimilarResponse));

            var category = new FaceCategory(countryCode, FaceGender.Female);

            this.mockFaceIdLookup
                .Setup(mock => mock.LookupName(fakePersistedFaceId1, category))
                .Returns("Maria");
            this.mockFaceIdLookup
                .Setup(mock => mock.LookupName(fakePersistedFaceId2, category))
                .Returns("Madalina");

            // Act
            FaceToNameMatchResult result = await unitUnderTest.MatchFaceToNameAsync(
                photo,
                countryCode,
                maxSimilarFaces);

            // Assert
            result.ShouldNotBeNull();
            result.Category.ShouldNotBeNull();
            result.Category.CountryCode.ShouldBe("ro");
            result.Category.Gender.ShouldBe(FaceGender.Female);
            result.Matches.ShouldNotBeNull();
            result.Matches.Count.ShouldBe(2);
            result.Matches[0].FirstName.ShouldBe("Maria");
            result.Matches[0].Score.ShouldBe(0.8765);
            result.Matches[1].FirstName.ShouldBe("Madalina");
            result.Matches[1].Score.ShouldBe(0.4321);
        }
    }
}
