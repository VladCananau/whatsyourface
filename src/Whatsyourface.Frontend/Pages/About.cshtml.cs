// <copyright file="About.cshtml.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.Pages
{
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            this.Message = "Your application description page.";
        }
    }
}
