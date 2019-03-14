// <copyright file="FaceCategory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using Dawn;

    public class FaceCategory
    {
        public string CountryCode { get; set; }
        public FaceGender Gender { get; set; }

        public FaceCategory(string countryCode, FaceGender gender)
        {
            Guard.Argument(countryCode, nameof(countryCode)).NotNull().NotWhiteSpace();
            this.CountryCode = countryCode;
            this.Gender = gender;
        }

        public override bool Equals(object obj)
        {
            FaceCategory other = obj as FaceCategory;
            return other != null
                && string.Equals(other.CountryCode, this.CountryCode)
                && other.Gender == this.Gender;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 11;
                h = h * 2357 + this.CountryCode.GetHashCode();
                h = h * 2357 + this.Gender.GetHashCode();
                return h;
            }
        }
    }
}
