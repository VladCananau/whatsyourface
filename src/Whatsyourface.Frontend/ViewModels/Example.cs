// <copyright file="Example.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class Example
    {
        [DataMember(Name = "imageFile")]
        public string ImageFile { get; set; }

        [DataMember(Name = "matches")]
        public IList<FaceToNameMatchViewModel> Matches { get; set; }
    }
}
