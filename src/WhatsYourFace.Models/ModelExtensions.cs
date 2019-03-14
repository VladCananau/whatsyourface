// <copyright file="ModelUtilities.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;

    public static class ModelExtensions
    {
        public static IEnumerable<FaceToNameMatch> SumScoresByName(this IEnumerable<FaceToNameMatch> matches)
        {
            return
                from match in matches
                group match by match.FirstName into matchGroup
                select new FaceToNameMatch(matchGroup.Key, matchGroup.Sum(s => s.Score));
        }

        public static IEnumerable<FaceToNameMatch> CountMatchesByName(this IEnumerable<FaceToNameMatch> matches)
        {
            return
                from match in matches
                group match by match.FirstName into matchGroup
                select new FaceToNameMatch(matchGroup.Key, matchGroup.Count());
        }

        public static IEnumerable<FaceToNameMatch> NormalizeScoresAsProbabilitySpace(this IList<FaceToNameMatch> matches)
        {
            double sumOfScores = matches.Sum(_ => _.Score);

            Guard.Argument(sumOfScores, nameof(matches)).Positive();

            foreach (FaceToNameMatch match in matches)
            {
                match.Score /= sumOfScores;
                yield return match;
            }
        }

        public static IEnumerable<FaceToNameMatch> NormalizeScoresAsProbabilitySpaceInPercentages(this IList<FaceToNameMatch> matches)
        {
            foreach (FaceToNameMatch match in matches.NormalizeScoresAsProbabilitySpace())
            {
                match.Score *= 100;
                yield return match;
            }
        }

        public static IEnumerable<FaceToNameMatch> AggregateMatchesByCountThenSum(this IEnumerable<FaceToNameMatch> matches)
        {
            // Every match will be worth 1 point + the value of its Score to the total score of the group
            // This works well when Score is a 0-1 confidence metric; it is meant to discriminate between 
            // groups of low confidence matches and groups of high confidence matches
            return
                from match in matches
                group match by match.FirstName into matchGroup
                select new FaceToNameMatch(matchGroup.Key, matchGroup.Count() + matchGroup.Sum(s => s.Score));
        }
    }
}
