// <copyright file="IFaceIdToNameLookup.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using WhatsYourFace.Models;

    public interface IFaceIdToNameLookup
    {
        string LookupName(Guid persistedFaceId, FaceCategory category);
    }
}