// <copyright file="FaceCategory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using System;
    using Dawn;

    public class FaceCategory
    {
        private readonly string countryCodeNormalized;

        public FaceCategory(string countryCode, FaceGender gender)
        {
            Guard.Argument(countryCode, nameof(countryCode)).NotNull().NotWhiteSpace();
            this.CountryCode = countryCode;
            this.countryCodeNormalized = countryCode.ToUpperInvariant();
            this.Gender = gender;
        }

        public string CountryCode { get; }

        public FaceGender Gender { get; }

        public override bool Equals(object obj)
        {
            return obj is FaceCategory other
                && string.Equals(other.CountryCode, this.CountryCode, StringComparison.OrdinalIgnoreCase)
                && other.Gender == this.Gender;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 11;
                h = (h * 2357) + this.countryCodeNormalized.GetHashCode();
                h = (h * 2357) + this.Gender.GetHashCode();
                return h;
            }
        }
    }
}
