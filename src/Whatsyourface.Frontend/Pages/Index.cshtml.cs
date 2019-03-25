// <copyright file="Index.cshtml.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.Pages
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using WhatsYourFace.Core;
    using WhatsYourFace.Frontend.ViewModels;
    using WhatsYourFace.Models;

#pragma warning disable SA1649 // File name must match first type name
    public class IndexModel : PageModel
#pragma warning restore SA1649
    {
        private readonly IFaceMatcher faceMatcher;
        private readonly ICannedExample cannedExample;
        private readonly ILogger<IndexModel> logger;

        public IndexModel(
            FaceMatchSettings faceMatchSettings,
            IFaceMatcher faceMatcher,
            ICannedExample cannedExample,
            ILogger<IndexModel> logger)
        {
            this.FaceMatchSettings = faceMatchSettings;
            this.faceMatcher = faceMatcher;
            this.cannedExample = cannedExample;
            this.logger = logger;
        }

        [BindProperty]
        [Required]
        public string CountryCode { get; set; }

        [BindProperty]
        public IFormFile UserImage { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only; Binding property
        [BindProperty]
        public IList<FaceToNameMatchViewModel> Matches { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        [BindProperty]
        public string ErrorMessage { get; set; }

        [BindProperty]
#pragma warning disable CA1056 // Uri properties should not be strings; Binding property
        public string ServerImageUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        public FaceMatchSettings FaceMatchSettings { get; }

        private string ErrorCode { get; set; }

        public bool HasResult()
        {
            return this.Matches != null || this.ErrorMessage != null;
        }

        public async Task OnPostAsync()
        {
            using (var scope = this.logger.BeginScope(new Dictionary<string, object> { { "CountryCode", this.CountryCode } }))
            {
                this.ErrorCode = null;
                this.ErrorMessage = null;
                this.Matches = null;

                if (this.IsCountryCodeMissing())
                {
                    this.ErrorMessage = Resources.Index.ErrorNoCountry;
                    this.ErrorCode = nameof(Resources.Index.ErrorNoCountry);
                }
                else if (!this.IsCountryCodeSupported())
                {
                    this.ErrorMessage = string.Format(
                        Resources.Index.ErrorCountryNotSupported,
                        this.CountryCode);
                    this.ErrorCode = nameof(Resources.Index.ErrorCountryNotSupported);
                }
                else if (this.IsImageMissing())
                {
                    this.ErrorMessage = Resources.Index.ErrorNoImage;
                    this.ErrorCode = nameof(Resources.Index.ErrorNoImage);
                }
                else if (this.IsCannedImageUsed())
                {
                    this.UseCannedExample();
                }
                else if (this.IsUserImageTooLarge())
                {
                    this.ErrorMessage = Resources.Index.ErrorFileTooLarge;
                    this.ErrorCode = nameof(Resources.Index.ErrorFileTooLarge);
                }
                else
                {
                    await this.UseUserImage();
                }

                this.LogPostOutcome();
            }
        }

        private bool IsUserImageTooLarge()
        {
            return this.UserImage != null && this.UserImage.Length > this.FaceMatchSettings.MaxImageSizeInBytes;
        }

        private bool IsCannedImageUsed()
        {
            return !string.IsNullOrWhiteSpace(this.ServerImageUrl);
        }

        private bool IsImageMissing()
        {
            return this.UserImage == null && string.IsNullOrWhiteSpace(this.ServerImageUrl);
        }

        private bool IsCountryCodeSupported()
        {
            return this.FaceMatchSettings.SupportedCountryCodes.Contains(this.CountryCode, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsCountryCodeMissing()
        {
            return string.IsNullOrWhiteSpace(this.CountryCode);
        }

        private void UseCannedExample()
        {
            try
            {
                this.Matches = this.cannedExample
                        .ExampleSets
                        .Single(set => string.Equals(set.CountryCode, this.CountryCode, StringComparison.OrdinalIgnoreCase))
                        .Examples
                        .Single(example => this.ServerImageUrl.EndsWith(example.ImageFile, StringComparison.OrdinalIgnoreCase))
                        .Matches;
            }
            catch (Exception)
            {
                this.Matches = null;
                this.ErrorMessage = Resources.Index.ErrorNoImage;
                this.ErrorCode = nameof(Resources.Index.ErrorNoImage);
            }
        }

        private async Task UseUserImage()
        {
            try
            {
                this.logger.LogInformation("The image uploaded by the user is {userImageSizeInBytes} bytes", this.UserImage.Length);
                using (Stream photoStream = this.UserImage.OpenReadStream())
                {
                    this.Matches =
                        (await this.faceMatcher.MatchFaceToNameAsync(photoStream, this.CountryCode, this.FaceMatchSettings.MaxNumberOfFaces))
                            .Matches
                            .AggregateMatchesByCountThenSum()
                            .OrderByDescending(match => match.Score)
                            .Take(10)
                            .ToList()
                            .NormalizeScoresAsProbabilitySpaceInPercentages()
                            .Take(3)
                            .Select(FaceToNameMatchViewModel.FromFaceToNameMatch)
                            .ToList();
                }
            }
            catch (FaceMatchException fmex)
            {
                if (fmex.ErrorCode == FaceMatchException.Code.MoreThanOneFaceInPhoto
                    || fmex.ErrorCode == FaceMatchException.Code.ZeroFacesInPhoto)
                {
                    this.ErrorMessage = Resources.Index.ErrorMatchingFaceToName;
                    this.ErrorCode = nameof(Resources.Index.ErrorMatchingFaceToName);
                    this.LogUserError(this.ErrorCode, this.ErrorMessage);
                }
                else
                {
                    this.ErrorMessage = Resources.Index.ErrorInternal;
                    this.ErrorCode = nameof(Resources.Index.ErrorInternal);
                    this.LogServiceError(this.ErrorCode, this.ErrorMessage, fmex);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = Resources.Index.ErrorInternal;
                this.ErrorCode = nameof(Resources.Index.ErrorInternal);
                this.LogServiceError(this.ErrorCode, this.ErrorMessage, ex);
            }
        }

        private void LogPostOutcome()
        {
            if (this.ErrorCode != null || this.ErrorMessage != null)
            {
                if (this.ErrorCode != nameof(Resources.Index.ErrorInternal))
                {
                    this.LogUserError(this.ErrorCode, this.ErrorMessage);
                }
            }
            else if (this.Matches != null && this.Matches.Count > 0)
            {
                this.logger.LogInformation(
                    "{outcome}: Returning {matchCount} matches",
                    "Success",
                    this.Matches.Count);
            }
        }

        private void LogServiceError(string errorCode, string errorMessage, Exception ex)
            => this.LogError(errorCode, errorMessage, "ServiceError", ex);

        private void LogUserError(string errorCode, string errorMessage)
            => this.LogError(errorCode, errorMessage, "UserError");

        private void LogError(string errorCode, string errorMessage, string errorType, Exception ex = null)
        {
            this.logger.LogError(ex, "{outcome} ({errorCode}): {errorMessage}", errorType, errorCode, errorMessage);
        }
    }
}
