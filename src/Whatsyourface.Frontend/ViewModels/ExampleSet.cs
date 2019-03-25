// <copyright file="ExampleSet.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class ExampleSet
    {
        [DataMember(Name = "countryCode")]
        public string CountryCode { get; set; }

        [DataMember(Name = "examples")]
        public IList<Example> Examples { get; set; }
    }
}
