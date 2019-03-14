// <copyright file="CoreUtilities.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using WhatsYourFace.Models;

    public static class CoreUtilities
    {
        public static bool IsSameAs(this Gender detectedGender, FaceGender statedGender)
        {
            return (detectedGender == Gender.Male && statedGender == FaceGender.Male)
                || (detectedGender == Gender.Female && statedGender == FaceGender.Female)
                || (detectedGender != Gender.Male && detectedGender != Gender.Female
                    && statedGender != FaceGender.Male && statedGender != FaceGender.Female);
        }

        public static FaceGender Convert(this Gender detectedGender)
        {
            switch (detectedGender)
            {
                case Gender.Male: { return FaceGender.Male; }
                case Gender.Female: { return FaceGender.Female; }
                default: { return FaceGender.None; }
            };
        }

        public static string ToFaceListId(this FaceCategory category, string format)
        {
            return format
                .Replace("{countrycode}", category.CountryCode.ToLowerInvariant())
                .Replace("{gender}", category.Gender.ToString().ToLowerInvariant());
        }
    }
}
