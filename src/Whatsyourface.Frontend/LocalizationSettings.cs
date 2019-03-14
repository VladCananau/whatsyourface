// <copyright file="LocalizationSettings.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend
{
    using Dawn;
    using System.Collections.Generic;

    public class LocalizationSettings
    {
        public string CultureCookieName { get; set; }

        public List<string> SupportedCultures { get; set; }

        public string DefaultCulture { get; set; }

        public void Validate()
        {
            Guard.Argument(this.CultureCookieName, nameof(this.CultureCookieName)).NotNull().NotWhiteSpace();
            Guard.Argument(this.DefaultCulture, nameof(this.DefaultCulture)).NotNull().NotWhiteSpace();
            Guard.Argument(this.SupportedCultures, nameof(this.SupportedCultures)).NotNull().NotEmpty();
        }
    }
}
