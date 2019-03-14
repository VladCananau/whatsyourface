// <copyright file="FaceToNameMatch.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using Dawn;

    public class FaceToNameMatch
    {
        public FaceToNameMatch(string firstName, double score)
        {
            Guard.Argument(firstName, nameof(firstName)).NotNull().NotWhiteSpace();
            Guard.Argument(score, nameof(score)).NotNegative();
            this.FirstName = firstName;
            this.Score = score;
        }

        public string FirstName { get; }

        public double Score { get; set; }
    }
}
