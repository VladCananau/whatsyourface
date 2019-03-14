// <copyright file="FaceToNameMatchResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;

    public class FaceToNameMatchResult
    {
        public FaceToNameMatchResult(FaceCategory category)
            : this(category, Enumerable.Empty<FaceToNameMatch>())
        {
        }

        public FaceToNameMatchResult(FaceCategory category, IEnumerable<FaceToNameMatch> matches)
        {
            Guard.Argument(matches, nameof(matches)).NotNull();
            Guard.Argument(category, nameof(category)).NotNull();
            this.Category = category;
            this.Matches = new List<FaceToNameMatch>(matches);
        }

        public FaceCategory Category { get; }

        public List<FaceToNameMatch> Matches { get; private set; }
    }
}
