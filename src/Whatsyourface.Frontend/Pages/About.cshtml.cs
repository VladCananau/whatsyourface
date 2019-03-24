// <copyright file="About.cshtml.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.Pages
{
    using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable SA1649 // File name must match first type name
    public class AboutModel : PageModel
#pragma warning restore SA1649 // File name must match first type name
    {
        public string Message { get; set; }

        public void OnGet()
        {
            this.Message = "Your application description page.";
        }
    }
}
