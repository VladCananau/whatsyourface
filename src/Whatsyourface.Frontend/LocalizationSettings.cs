// <copyright file="LocalizationSettings.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend
{
    using System.Collections.Generic;
    using Dawn;

    public class LocalizationSettings
    {
        public string CultureCookieName { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only; Deserialization
        public List<string> SupportedCultures { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public string DefaultCulture { get; set; }

        public void Validate()
        {
            Guard.Argument(this.CultureCookieName, nameof(this.CultureCookieName)).NotNull().NotWhiteSpace();
            Guard.Argument(this.DefaultCulture, nameof(this.DefaultCulture)).NotNull().NotWhiteSpace();
            Guard.Argument(this.SupportedCultures, nameof(this.SupportedCultures)).NotNull().NotEmpty();
        }
    }
}
