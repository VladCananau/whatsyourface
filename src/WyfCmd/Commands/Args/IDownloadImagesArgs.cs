// <copyright file="IDownloadImagesArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using CommandLine;
    using PowerArgs;

    public interface IDownloadImagesArgs
    {
        [Option('l', "namelist", SetName = "namelist", Required = true, HelpText = "A csv file containing a list of names for which to download images.")]
        [ArgShortcut("l")]
        [ArgShortcut("--namelist")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgCantBeCombinedWith("name")]
        [ArgDescription("The id of the new face list.")]
        string NameList { get; set; }

        [Option('n', "name", SetName = "name", Required = true, HelpText = "The name for which to download images.")]
        [ArgShortcut("n")]
        [ArgShortcut("--name")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgCantBeCombinedWith("namelist")]
        [ArgDescription("The name for which to download images.")]
        string Name { get; set; }

        [Option('x', "count", Required = true, HelpText = "The number of images to upload for each name.")]
        [ArgShortcut("x")]
        [ArgShortcut("--count")]
        [ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The number of images to upload for each name.")]
        int Count { get; set; }

        [Option('s', "skip", Default = 0, HelpText = "The number of images to skip for each name before starting to download (use this so that you dont' download the same images twice).")]
        [ArgShortcut("s")]
        [ArgDefaultValue(0)]
        [ArgDescription("The number of images to skip for each name before starting to download (use this so that you dont' download the same images twice).")]
        int Skip { get; set; }

        [Option('c', "country", Required = true, HelpText = "The 2-letter country code for the country from where the names and images should be.")]
        [ArgShortcut("c")]
        [ArgShortcut("--country")]
        [ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The 2-letter country code for the country from where the names and images should be.")]
        string CountryCode { get; set; }

        [Option('o', "outputfolder", Required = true, HelpText = "The root folder where the downloaded images will be stored, in one subfolder per each name in --namelist.")]
        [ArgShortcut("o")]
        [ArgShortcut("--outputfolder")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The root folder where the downloaded images will be stored, in one subfolder per each name in --namelist.")]
        string OutputFolder { get; set; }
    }
}