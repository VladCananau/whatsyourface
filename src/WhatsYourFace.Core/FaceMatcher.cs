// <copyright file="FaceMatcher.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Microsoft.Extensions.Logging;
    using WhatsYourFace.Models;

    public class FaceMatcher : IFaceMatcher
    {
        private const int FindSimilarMaxNumberOfResults = 1000; // API limit

        public FaceMatcher(
            IFaceClient faceClient,
            IFaceIdToNameLookup faceIdLookup,
            FaceMatchSettings settings,
            ILogger<FaceMatcher> logger)
        {
            Guard.Argument(faceClient, nameof(faceClient)).NotNull();
            Guard.Argument(settings, nameof(settings)).NotNull();
            Guard.Argument(logger, nameof(logger)).NotNull();

            this.FaceClient = faceClient;
            this.FaceIdLookup = faceIdLookup;
            this.Settings = settings;
            this.Logger = logger;
        }

        protected IFaceClient FaceClient { get; }

        protected FaceMatchSettings Settings { get; }

        protected ILogger<FaceMatcher> Logger { get; }

        protected IFaceIdToNameLookup FaceIdLookup { get; }

        public void Dispose()
        {
            this.FaceClient.Dispose();
        }

        public async Task<DetectedFace> DetectSingleFaceAsync(Stream photo)
        {
            IList<DetectedFace> faces
                = await this.FaceClient.Face.DetectWithStreamAsync(
                            photo,
                            returnFaceId: true,
                            returnFaceLandmarks: false,
                            new[] { FaceAttributeType.Gender });

            if (faces.Count() != 1)
            {
                var exceptionCode = faces.Count == 0
                    ? FaceMatchException.Codes.ZeroFacesInPhoto
                    : FaceMatchException.Codes.MoreThanOneFaceInPhoto;
                throw new FaceMatchException(
                    exceptionCode,
                    $"The image contains {faces.Count()} faces instead of 1");
            }

            this.Logger.LogInformation(
                "Detected a {gender} face with faceId '{faceId}'",
                faces[0].FaceAttributes.Gender,
                faces[0].FaceId);
            return faces[0];
        }

        public async Task<FaceToNameMatchResult> MatchFaceToNameAsync(
            Stream photo,
            string countryCode,
            int maxSimilarFaces)
        {
            Guard.Argument(photo, nameof(photo)).NotNull();
            Guard.Argument(countryCode, nameof(countryCode)).NotNull().NotWhiteSpace();
            Guard.Argument(maxSimilarFaces, nameof(maxSimilarFaces)).InRange(1, 1000);

            DetectedFace face = await this.DetectSingleFaceAsync(photo);
            FaceCategory category = GetFaceCategory(face, countryCode);

            using (this.Logger.BeginScope(
                new Dictionary<string, object> { { "gender", category.Gender } }))
            {
                IEnumerable<FaceToNameMatch> matches
                    = await this.MatchFaceToNameAsync(face, category, maxSimilarFaces);

                FaceToNameMatchResult result = new FaceToNameMatchResult(category);
                result.Matches.AddRange(matches);
                return result;
            }
        }

        protected async Task<IList<SimilarFace>> FindSimilarFacesAsync(
            Guid faceId,
            FaceCategory matchCategory,
            int maxResults)
        {
            Guard.Argument(matchCategory, nameof(matchCategory)).NotNull();
            Guard.Argument(maxResults, nameof(maxResults)).InRange(1, FindSimilarMaxNumberOfResults);

            return await this.FaceClient.Face.FindSimilarAsync(
                faceId,
                faceListId: matchCategory.ToFaceListId(this.Settings.FaceListNameFormat),
                maxNumOfCandidatesReturned: maxResults,
                mode: FindSimilarMatchMode.MatchFace);
        }

        private static FaceCategory GetFaceCategory(DetectedFace face, string countryCode)
        {
            Debug.Assert(face.FaceAttributes.Gender.HasValue, "Face detection did not return a Gender");
            FaceCategory searchCategory
                = new FaceCategory(countryCode, face.FaceAttributes.Gender.Value.Convert());
            return searchCategory;
        }

        private async Task<IEnumerable<FaceToNameMatch>> MatchFaceToNameAsync(
            DetectedFace face,
            FaceCategory category,
            int maxSimilarFaces)
        {
            IList<SimilarFace> similarFaces
                = await this.FindSimilarFacesAsync(face.FaceId.Value, category, maxSimilarFaces);

            this.Logger.LogInformation($"Retrieved {similarFaces.Count} similar faces.");

            IEnumerable<FaceToNameMatch> matches = this.ConvertFaceIdsToNames(similarFaces, category, this.FaceIdLookup);
            return matches;
        }

        private IList<FaceToNameMatch> ConvertFaceIdsToNames(
            IList<SimilarFace> similarFaces,
            FaceCategory lookupCategory,
            IFaceIdToNameLookup faceIdLookup)
        {
            IEnumerable<FaceToNameMatch> converted =
                from result in similarFaces
                select new FaceToNameMatch(
                    firstName: faceIdLookup.LookupName(result.PersistedFaceId.Value, lookupCategory),
                    score: result.Confidence);

            return converted.ToList();
        }
    }
}
