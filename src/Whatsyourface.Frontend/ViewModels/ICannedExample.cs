// <copyright file="ICannedExample.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Frontend.ViewModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public interface ICannedExample
    {
        IList<ExampleSet> ExampleSets { get; }
    }
}
