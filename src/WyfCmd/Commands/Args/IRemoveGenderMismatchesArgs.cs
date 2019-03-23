// <copyright file="IRemoveGenderMismatchesArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using CommandLine;
    using PowerArgs;
    using WhatsYourFace.Models;

    public interface IRemoveGenderMismatchesArgs
    {
        [Option('i', "images", Required = true, HelpText = "A folder containing images of faces.")]
        [ArgShortcut("i")]
        [ArgShortcut("--images")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("A folder containing images of faces.")]
        string ImagesFolder { get; set; }

        [Option('g', "gender", Required = true, HelpText = "The expected gender of all the faces in the folder.")]
        [ArgShortcut("g")]
        [ArgShortcut("--gender")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("The expected gender of all the faces in the folder.")]
        FaceGender Gender { get; set; }

        [Option('q', "quarantine", Required = true, HelpText = "A folder where we will move all the faces of a different gender than expected.")]
        [ArgShortcut("q")]
        [ArgShortcut("--quarantine")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("A folder where we will move all the faces of a different gender than expected.")]
        string QuarantineFolder { get; set; }
    }
}