// <copyright file="FaceMatchSettings.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System.Collections.Generic;

    public class FaceMatchSettings
    {
        public int MaxNumberOfFaces { get; set; }

        public string FaceListNameFormat { get; set; }

        public List<string> SupportedCountryCodes { get; set; }

        public int MaxImageSizeInBytes { get; set; }
    }
}
