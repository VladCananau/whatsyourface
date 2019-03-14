// <copyright file="IImageSanitizer.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System.Threading.Tasks;
    using WhatsYourFace.Models;

    public interface IImageSanitizer
    {
        Task RemoveGenderMismatches(string imagesDirectory, FaceGender expectedGender, string quarantineDirectory);
    }
}