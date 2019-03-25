// <copyright file="FaceToNameMatchViewModel.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using WhatsYourFace.Models;

    public class FaceToNameMatchViewModel
    {
        public FaceToNameMatchViewModel(string firstName, double score)
        {
            this.FirstName = firstName;
            this.Score = score;
        }

        public string FirstName { get; }

        public double Score { get; set; }

        public static FaceToNameMatchViewModel FromFaceToNameMatch(FaceToNameMatch source)
        {
            return new FaceToNameMatchViewModel(source.FirstName, source.Score);
        }
    }
}
