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

        private readonly IFaceClient faceClient;

        private readonly FaceMatchSettings settings;

        private readonly ILogger<FaceMatcher> logger;

        private readonly IFaceIdToNameLookup faceIdLookup;

        public FaceMatcher(
            IFaceClient faceClient,
            IFaceIdToNameLookup faceIdLookup,
            FaceMatchSettings settings,
            ILogger<FaceMatcher> logger)
        {
            Guard.Argument(faceClient, nameof(faceClient)).NotNull();
            Guard.Argument(settings, nameof(settings)).NotNull();
            Guard.Argument(logger, nameof(logger)).NotNull();
            Guard.Argument(faceIdLookup, nameof(faceIdLookup)).NotNull();

            this.faceClient = faceClient;
            this.faceIdLookup = faceIdLookup;
            this.settings = settings;
            this.logger = logger;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<DetectedFace> DetectSingleFaceAsync(Stream photo)
        {
            IList<DetectedFace> faces
                = await this.faceClient.Face.DetectWithStreamAsync(
                            photo,
                            returnFaceId: true,
                            returnFaceLandmarks: false,
                            new[] { FaceAttributeType.Gender }).ConfigureAwait(false);

            if (faces.Count() != 1)
            {
                var exceptionCode = faces.Count == 0
                    ? FaceMatchException.Code.ZeroFacesInPhoto
                    : FaceMatchException.Code.MoreThanOneFaceInPhoto;
                throw new FaceMatchException(
                    exceptionCode,
                    $"The image contains {faces.Count()} faces instead of 1");
            }

            this.logger.LogInformation(
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

            DetectedFace face = await this.DetectSingleFaceAsync(photo).ConfigureAwait(false);
            FaceCategory category = GetFaceCategory(face, countryCode);

            using (this.logger.BeginScope(
                new Dictionary<string, object> { { "gender", category.Gender } }))
            {
                IEnumerable<FaceToNameMatch> matches
                    = await this.MatchFaceToNameAsync(face, category, maxSimilarFaces).ConfigureAwait(false);

                FaceToNameMatchResult result = new FaceToNameMatchResult(category);
                result.Matches.AddRange(matches);
                return result;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.faceClient != null)
                {
                    this.faceClient.Dispose();
                }
            }
        }

        protected async Task<IList<SimilarFace>> FindSimilarFacesAsync(
            Guid faceId,
            FaceCategory matchCategory,
            int maxResults)
        {
            Guard.Argument(matchCategory, nameof(matchCategory)).NotNull();
            Guard.Argument(maxResults, nameof(maxResults)).InRange(1, FindSimilarMaxNumberOfResults);

            return await this.faceClient.Face.FindSimilarAsync(
                faceId,
                faceListId: matchCategory.ToFaceListId(this.settings.FaceListNameFormat),
                maxNumOfCandidatesReturned: maxResults,
                mode: FindSimilarMatchMode.MatchFace).ConfigureAwait(false);
        }

        private static FaceCategory GetFaceCategory(DetectedFace face, string countryCode)
        {
            Debug.Assert(face.FaceAttributes.Gender.HasValue, "Face detection did not return a Gender");
            FaceCategory searchCategory
                = new FaceCategory(countryCode, face.FaceAttributes.Gender.Value.Convert());
            return searchCategory;
        }

        private static IList<FaceToNameMatch> ConvertFaceIdsToNames(
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

        private async Task<IEnumerable<FaceToNameMatch>> MatchFaceToNameAsync(
            DetectedFace face,
            FaceCategory category,
            int maxSimilarFaces)
        {
            IList<SimilarFace> similarFaces
                = await this.FindSimilarFacesAsync(face.FaceId.Value, category, maxSimilarFaces).ConfigureAwait(false);

            this.logger.LogInformation($"Retrieved {similarFaces.Count} similar faces.");

            IEnumerable<FaceToNameMatch> matches = ConvertFaceIdsToNames(similarFaces, category, this.faceIdLookup);
            return matches;
        }
    }
}
