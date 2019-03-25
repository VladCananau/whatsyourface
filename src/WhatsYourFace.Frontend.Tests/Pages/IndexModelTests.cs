// <copyright file="IndexModelTests.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.Tests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Shouldly;
    using WhatsYourFace.Core;
    using WhatsYourFace.Frontend.Pages;
    using WhatsYourFace.Frontend.ViewModels;
    using WhatsYourFace.Models;
    using Xunit;

    public sealed class IndexModelTests : IDisposable
    {
        private readonly MockRepository mockRepository;

        private readonly FaceMatchSettings fakeFaceMatchSettings;
        private readonly Mock<IFaceMatcher> mockFaceMatcher;
        private readonly Mock<ICannedExample> mockCannedExample;
        private readonly Mock<IFormFile> mockUserImage;
        private readonly Mock<ILogger<IndexModel>> mockLogger;

        public IndexModelTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.fakeFaceMatchSettings = new FaceMatchSettings
            {
                MaxNumberOfFaces = 10,
                MaxImageSizeInBytes = 1234,
                SupportedCountryCodes = new List<string> { "ro", "ru" }
            };

            this.mockFaceMatcher = this.mockRepository.Create<IFaceMatcher>();
            this.mockCannedExample = this.mockRepository.Create<ICannedExample>();
            this.mockUserImage = this.mockRepository.Create<IFormFile>();

            this.mockLogger = this.mockRepository.Create<ILogger<IndexModel>>(MockBehavior.Loose);
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public void HasResult_IsFalse()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.Matches = null;
            unitUnderTest.ErrorMessage = null;

            // Act
            bool result = unitUnderTest.HasResult();

            // Assert
            result.ShouldBe(false);
        }

        [Fact]
        public void HasResult_IsTrue_HasMatches()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.Matches = new List<FaceToNameMatchViewModel>();
            unitUnderTest.ErrorMessage = null;

            // Act
            bool result = unitUnderTest.HasResult();

            // Assert
            result.ShouldBe(true);
        }

        [Fact]
        public void HasResult_IsTrue_HasErrorMessage()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.Matches = null;
            unitUnderTest.ErrorMessage = "Big mistake";

            // Act
            bool result = unitUnderTest.HasResult();

            // Assert
            result.ShouldBe(true);
        }

        [Fact]
        public async Task OnPostAsync_CannedExample_Success()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ro";
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/ex1.png";
            unitUnderTest.UserImage = null;
            this.mockCannedExample.Setup(mock => mock.ExampleSets).Returns(this.BuildCannedExample().ExampleSets);

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches.ShouldNotBeNull();
            unitUnderTest.Matches.Count.ShouldBe(2);
            unitUnderTest.Matches[0].FirstName.ShouldBe("Ion");
            unitUnderTest.Matches[0].Score.ShouldBe(42.42);
            unitUnderTest.Matches[1].FirstName.ShouldBe("Gheorghe");
            unitUnderTest.Matches[1].Score.ShouldBe(21.84);
            unitUnderTest.ErrorMessage.ShouldBeNull();
        }

        [Fact]
        public async Task OnPostAsync_CannedExample_NotFound()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/wrong.png";
            unitUnderTest.UserImage = null;
            this.mockCannedExample.Setup(mock => mock.ExampleSets).Returns(this.BuildCannedExample().ExampleSets);

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches.ShouldBeNull();
            unitUnderTest.ErrorMessage.ShouldBe(Resources.Index.ErrorNoImage);
        }

        [Fact]
        public async Task OnPostAsync_UserImage_Success()
        {
            // Arrange
            const string CountryCode = "ro";
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = CountryCode;
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = this.mockUserImage.Object;
            this.SetupMockUserImage(this.mockUserImage);

            MemoryStream fakeImageStream = new MemoryStream();
            this.mockUserImage.Setup(mock => mock.OpenReadStream()).Returns(fakeImageStream);

            FaceToNameMatchResult fakeResult =
                new FaceToNameMatchResult(new FaceCategory(CountryCode, FaceGender.Male));
            fakeResult.Matches.AddRange(new[]
            {
                new FaceToNameMatch("Florin", 69.5),
                new FaceToNameMatch("Tiberiu", 28.5)
            });

            this.mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(
                    fakeImageStream, CountryCode, this.fakeFaceMatchSettings.MaxNumberOfFaces))
                .Returns(Task.FromResult(fakeResult));

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches.ShouldNotBeNull();
            unitUnderTest.Matches.Count.ShouldBe(2);
            unitUnderTest.Matches[0].FirstName.ShouldBe("Florin");
            unitUnderTest.Matches[0].Score.ShouldNotBe(0);
            unitUnderTest.Matches[1].FirstName.ShouldBe("Tiberiu");
            unitUnderTest.Matches[1].Score.ShouldNotBe(0);
            unitUnderTest.ErrorMessage.ShouldBeNull();
        }

        [Fact]
        public async Task OnPostAsync_UserImage_Success_NormalizeResults()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = this.mockUserImage.Object;
            this.SetupMockUserImage(this.mockUserImage);

            FaceToNameMatchResult fakeResult =
                new FaceToNameMatchResult(new FaceCategory("ru", FaceGender.Female));
            fakeResult.Matches.AddRange(new[]
            {
                new FaceToNameMatch("First", 0.20),
                new FaceToNameMatch("Second", 0.80)
            });

            this.mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(fakeResult));

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches[0].FirstName.ShouldBe("Second");
            unitUnderTest.Matches[0].Score.ShouldBe(60.0);
            unitUnderTest.Matches[1].FirstName.ShouldBe("First");
            unitUnderTest.Matches[1].Score.ShouldBe(40.0);
        }

        [Fact]
        public async Task OnPostAsync_UserImage_FaceMatchException()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ro";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = this.mockUserImage.Object;
            this.SetupMockUserImage(this.mockUserImage);

            this.mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new FaceMatchException(FaceMatchException.Code.ZeroFacesInPhoto, "Matching went pear-shaped"));

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches.ShouldBeNull();
            unitUnderTest.ErrorMessage.ShouldBe(Resources.Index.ErrorMatchingFaceToName);
        }

        [Fact]
        public async Task OnPostAsync_UserImage_InternalException()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = this.mockUserImage.Object;
            this.SetupMockUserImage(this.mockUserImage);

            this.mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception("Something stupid like 'I love you'"));

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.Matches.ShouldBeNull();
            unitUnderTest.ErrorMessage.ShouldBe(Resources.Index.ErrorInternal);
        }

        [Fact]
        public async Task OnPostAsync_Error_NoCountryCode()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = string.Empty;
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/ex1.png";
            unitUnderTest.UserImage = null;

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.ErrorMessage.ShouldBe(Resources.Index.ErrorNoCountry);
            unitUnderTest.Matches.ShouldBeNull();
        }

        [Fact]
        public async Task OnPostAsync_Error_NoImage()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "ro";
            unitUnderTest.ServerImageUrl = string.Empty;
            unitUnderTest.UserImage = null;

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.ErrorMessage.ShouldBe(Resources.Index.ErrorNoImage);
            unitUnderTest.Matches.ShouldBeNull();
        }

        [Fact]
        public async Task OnPostAsync_Error_CountryCodeNotSupported()
        {
            // Arrange
            IndexModel unitUnderTest = this.CreateIndexModel();
            unitUnderTest.CountryCode = "xy";
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/ex1.png";
            unitUnderTest.UserImage = null;

            // Act
            await unitUnderTest.OnPostAsync();

            // Assert
            unitUnderTest.ErrorMessage.ShouldBe(string.Format(Resources.Index.ErrorCountryNotSupported, "xy"));
            unitUnderTest.Matches.ShouldBeNull();
        }

        private CannedExample BuildCannedExample() => new CannedExample()
        {
            ExampleSets = new[]
            {
                new ExampleSet
                {
                    CountryCode = "ru",
                    Examples = new[]
                    {
                        new Example
                        {
                            ImageFile = "ex1.png",
                            Matches = new[]
                            {
                                new FaceToNameMatchViewModel("Ivan", 42.42),
                                new FaceToNameMatchViewModel("Gyorgy", 21.84)
                            }
                        }
                    }
                },
                new ExampleSet
                {
                    CountryCode = "ro",
                    Examples = new[]
                    {
                        new Example
                        {
                            ImageFile = "ex1.png",
                            Matches = new[]
                            {
                                new FaceToNameMatchViewModel("Ion", 42.42),
                                new FaceToNameMatchViewModel("Gheorghe", 21.84)
                            }
                        },
                        new Example
                        {
                            ImageFile = "ex2.jpg",
                            Matches = new[]
                            {
                                new FaceToNameMatchViewModel("Vasile", 87.65),
                                new FaceToNameMatchViewModel("Dumitru", 12.34)
                            }
                        }
                    }
                }
            }
        };

        private void SetupMockUserImage(Mock<IFormFile> mockImage)
        {
            mockImage.SetupGet(mock => mock.Length).Returns(10);
            Stream fakeStream = Stream.Null;
            mockImage.Setup(mock => mock.OpenReadStream()).Returns(fakeStream);
        }

        private IndexModel CreateIndexModel()
        {
            return new IndexModel(
                this.fakeFaceMatchSettings,
                this.mockFaceMatcher.Object,
                this.mockCannedExample.Object,
                this.mockLogger.Object);
        }
    }
}
