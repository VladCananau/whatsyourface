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

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> SupportedCountryCodes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public int MaxImageSizeInBytes { get; set; }
    }
}
