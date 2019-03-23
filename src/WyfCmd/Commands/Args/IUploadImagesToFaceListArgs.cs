// <copyright file="IUploadImagesToFaceListArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using CommandLine;
    using PowerArgs;
    using WhatsYourFace.Models;

    public interface IUploadImagesToFaceListArgs
    {
        [Option('l', "namelist", SetName = "namelist", Required = true, HelpText = "A csv file containing a list of names for which to upload images.")]
        [ArgShortcut("l")]
        [ArgShortcut("--namelist")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgCantBeCombinedWith(nameof(Name))]
        [ArgDescription("A csv file containing a list of names for which to upload images.")]
        string NameList { get; set; }

        [Option('n', "name", SetName = "name", Required = true, HelpText = "The name for which to upload images.")]
        [ArgShortcut("n")]
        [ArgShortcut("--name")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgCantBeCombinedWith(nameof(NameList))]
        [ArgDescription("The name for which to upload images.")]
        string Name { get; set; }

        [Option('i', "images", Required = true, HelpText = "Folder containing a subfolder with images for every name in the --namelist, or for the single --name.")]
        [ArgShortcut("i")]
        [ArgShortcut("--images")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Folder containing a subfolder with images for every name in the --namelist, or for the single --name.")]
        string ImagesFolder { get; set; }

        [Option('x', "count", Required = true, HelpText = "The number of images to upload for each name.")]
        [ArgShortcut("x")]
        [ArgShortcut("--count")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The number of images to upload for each name.")]
        int Count { get; set; }

        [Option('s', "skip", Default = 0, HelpText = "The number of images to skip for each name before starting to upload.")]
        [ArgShortcut("s")]
        [ArgShortcut("--skip")]
        [ArgDefaultValue(0)]
        [ArgDescription("The number of images to skip for each name before starting to upload.")]
        int Skip { get; set; }

        [Option('g', "gender", Required = true, HelpText = "The gender of the person in the image.")]
        [ArgShortcut("g")]
        [ArgShortcut("--gender")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The gender of the person in the image.")]
        FaceGender Gender { get; set; }

        [Option('c', "country", Required = true, HelpText = "The 2-letter country code for the country from where the names and images are.")]
        [ArgShortcut("c")]
        [ArgShortcut("--country")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The 2-letter country code for the country from where the names and images are.")]
        string CountryCode { get; set; }

        [Option('f', "facelistid", Required = true, HelpText = "The id of the Cognitive Services face list to which to upload.")]
        [ArgShortcut("f")]
        [ArgShortcut("--facelistid")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The id of the Cognitive Services face list to which to upload.")]
        string FaceListId { get; set; }

        [Option('o', "outputfile", Required = true, HelpText = "A csv file to which the persistedFaceIds for each name will be written.")]
        [ArgShortcut("o")]
        [ArgShortcut("--outputfile")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("A csv file to which the persistedFaceIds for each name will be written.")]
        string Output { get; set; }
    }
}