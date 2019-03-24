// <copyright file="IMatchFaceToNamesArgs.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using CommandLine;
    using PowerArgs;

    public interface IMatchFaceToNamesArgs
    {
        [Option('i', "image", Required = true, HelpText = "The full path of the file containing your photo.")]
        [ArgShortcut("i")]
        [ArgShortcut("--image")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The full path of the file containing your photo.")]
        string ImageFilePath { get; set; }

        [Option('c', "country", Required = true, HelpText = "The 2-letter code of the country in which you want to know what's your face.")]
        [ArgShortcut("c")]
        [ArgShortcut("--country")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The 2-letter code of the country in which you want to know what's your face.")]
        string CountryCode { get; set; }

        [Option('m', "maxcandidates", Default = 50, HelpText = "The maximum number of the most similar faces to take into consideration")]
        [ArgShortcut("m")]
        [ArgShortcut("--maxcandidates")]
        [ArgDefaultValue(50)]
        [ArgDescription("The maximum number of the most similar faces to take into consideration")]
        int MaxCandidateFaces { get; set; }

        [Option('a', "aggregation", Default = ResultAggregation.None, HelpText = "Aggregate the results or leave them as they are.")]
        [ArgShortcut("a")]
        [ArgShortcut("--aggregation")]
        [ArgDefaultValue(ResultAggregation.None)]
        [ArgDescription("Aggregate the results or leave them as they are.")]
        ResultAggregation Aggregation { get; set; }
    }
}