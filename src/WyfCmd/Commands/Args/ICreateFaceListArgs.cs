// <copyright file="ICreateFaceListArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using CommandLine;
    using PowerArgs;

    public interface ICreateFaceListArgs
    {
        [Option('i', "id", Required = true, HelpText = "The id of the new face list.")]
        [ArgShortcut("i"), ArgRequired(PromptIfMissing = true), ArgDescription("The id of the new face list.")]
        string Id { get; set; }

        [Option('n', "name", Required = true, HelpText = "The name of the new face list.")]
        [ArgShortcut("n"), ArgRequired(PromptIfMissing = true), ArgDescription("The name of the new face list.")]
        string Name { get; set; }
    }
}