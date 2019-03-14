// <copyright file="IFaceMatcher.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using WhatsYourFace.Models;

    public interface IFaceMatcher : IDisposable
    {
        Task<FaceToNameMatchResult> MatchFaceToNameAsync(Stream photo, string countryCode, int maxSimilarFaces);
    }
}