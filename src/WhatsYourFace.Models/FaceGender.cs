// <copyright file="FaceGender.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Models
{
    using System;

    [Flags]
    public enum FaceGender
    {
        None = 0,
        Female = 1,
        Male = 2,
        Fluid = 3,
    }
}
