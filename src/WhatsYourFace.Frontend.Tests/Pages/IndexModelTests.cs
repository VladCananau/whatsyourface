using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WhatsYourFace.Core;
using WhatsYourFace.Frontend.Pages;
using WhatsYourFace.Frontend.ViewModels;
using WhatsYourFace.Models;
using Xunit;

namespace WhatsYourFace.Frontend.Tests.Pages
{
    public class IndexModelTests : IDisposable
    {
        private MockRepository mockRepository;

        private FaceMatchSettings fakeFaceMatchSettings;
        private Mock<IFaceMatcher> mockFaceMatcher;
        private Mock<ICannedExample> mockCannedExample;
        private Mock<IFormFile> mockUserImage;
        private Mock<ILogger<IndexModel>> mockLogger;

        public IndexModelTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);

            fakeFaceMatchSettings = new FaceMatchSettings();
            fakeFaceMatchSettings.MaxNumberOfFaces = 10;
            fakeFaceMatchSettings.MaxImageSizeInBytes = 1234;
            fakeFaceMatchSettings.SupportedCountryCodes = new List<string> { "ro", "ru" };

            mockFaceMatcher = mockRepository.Create<IFaceMatcher>();
            mockCannedExample = mockRepository.Create<ICannedExample>();
            mockUserImage = mockRepository.Create<IFormFile>();

            this.mockLogger = mockRepository.Create<ILogger<IndexModel>>(MockBehavior.Loose);
        }

        public void Dispose()
        {
            mockRepository.VerifyAll();
        }

        private IndexModel CreateIndexModel()
        {
            return new IndexModel(
                fakeFaceMatchSettings,
                mockFaceMatcher.Object,
                mockCannedExample.Object,
                mockLogger.Object);
        }

        [Fact]
        public void HasResult_IsFalse()
        {
            // Arrange
            IndexModel unitUnderTest = CreateIndexModel();
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
            IndexModel unitUnderTest = CreateIndexModel();
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
            IndexModel unitUnderTest = CreateIndexModel();
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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = "ro";
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/ex1.png";
            unitUnderTest.UserImage = null;
            mockCannedExample.Setup(mock => mock.ExampleSets).Returns(BuildCannedExample().ExampleSets);

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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = "http://wyf.com/images/wrong.png";
            unitUnderTest.UserImage = null;
            mockCannedExample.Setup(mock => mock.ExampleSets).Returns(BuildCannedExample().ExampleSets);

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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = CountryCode;
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = mockUserImage.Object;
            this.SetupMockUserImage(mockUserImage);

            MemoryStream fakeImageStream = new MemoryStream();
            mockUserImage.Setup(mock => mock.OpenReadStream()).Returns(fakeImageStream);

            FaceToNameMatchResult fakeResult = 
                new FaceToNameMatchResult(new FaceCategory(CountryCode, FaceGender.Male));
            fakeResult.Matches.AddRange(new[]
            {
                new FaceToNameMatch("Florin", 69.5),
                new FaceToNameMatch("Tiberiu", 28.5)
            });

            mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(
                    fakeImageStream, CountryCode, fakeFaceMatchSettings.MaxNumberOfFaces))
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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = mockUserImage.Object;
            this.SetupMockUserImage(mockUserImage);

            FaceToNameMatchResult fakeResult =
                new FaceToNameMatchResult(new FaceCategory("ru", FaceGender.Female));
            fakeResult.Matches.AddRange(new[]
            {
                new FaceToNameMatch("First", 0.20),
                new FaceToNameMatch("Second", 0.80)
            });

            mockFaceMatcher
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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = "ro";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = mockUserImage.Object;
            this.SetupMockUserImage(mockUserImage);

            mockFaceMatcher
                .Setup(mock => mock.MatchFaceToNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new FaceMatchException(FaceMatchException.Codes.ZeroFacesInPhoto, "Matching went pear-shaped"));

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
            IndexModel unitUnderTest = CreateIndexModel();
            unitUnderTest.CountryCode = "ru";
            unitUnderTest.ServerImageUrl = null;
            unitUnderTest.UserImage = mockUserImage.Object;
            this.SetupMockUserImage(mockUserImage);

            mockFaceMatcher
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
            IndexModel unitUnderTest = CreateIndexModel();
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
            IndexModel unitUnderTest = CreateIndexModel();
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
            IndexModel unitUnderTest = CreateIndexModel();
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
                new CannedExample.ExampleSet
                {
                    CountryCode = "ru",
                    Examples = new[]
                    {
                        new CannedExample.Example
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
                new CannedExample.ExampleSet
                {
                    CountryCode = "ro",
                    Examples = new[]
                    {
                        new CannedExample.Example
                        {
                            ImageFile = "ex1.png",
                            Matches = new[]
                            {
                                new FaceToNameMatchViewModel("Ion", 42.42),
                                new FaceToNameMatchViewModel("Gheorghe", 21.84)
                            }
                        },
                        new CannedExample.Example
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
    }
}
