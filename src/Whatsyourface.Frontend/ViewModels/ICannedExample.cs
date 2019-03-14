// <copyright file="CannedExample.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public interface ICannedExample
    {
        IList<CannedExample.ExampleSet> ExampleSets { get; set; }
    }

    public class CannedExample : ICannedExample
    {
        [DataMember(Name = "exampleSets")]
        public IList<ExampleSet> ExampleSets { get; set; }

        public static ICannedExample Deserialize(string jsonContent)
        {
            return JsonConvert.DeserializeObject<CannedExample>(jsonContent);
        }

        public class ExampleSet
        {
            [DataMember(Name = "countryCode")]
            public string CountryCode { get; set; }

            [DataMember(Name = "examples")]
            public IList<Example> Examples { get; set; }
        }

        public class Example
        {
            [DataMember(Name = "imageFile")]
            public string ImageFile { get; set; }

            [DataMember(Name = "matches")]
            public IList<FaceToNameMatchViewModel> Matches { get; set; }
        }
    }
}
