// <copyright file="CannedExample.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class CannedExample : ICannedExample
    {
        [DataMember(Name = "exampleSets")]
        public IList<ExampleSet> ExampleSets { get; set; }

        public static ICannedExample Deserialize(string jsonContent)
        {
            return JsonConvert.DeserializeObject<CannedExample>(jsonContent);
        }
    }
}
