// <copyright file="CreateFaceListCmd.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using CommandLine;
    using Dawn;
    using Microsoft.Azure.CognitiveServices.Vision.Face;

    [Verb("createfacelist", HelpText = "Create a new face list in Cognitive Services.")]
    public class CreateFaceListCmd : CmdBase<ICreateFaceListArgs>, ICreateFaceListArgs
    {
        private readonly IFaceClient faceClient;

        public CreateFaceListCmd()
        {
        }

        public CreateFaceListCmd(IFaceClient faceClient, IFileSystem fileSystem, Wyfcmd.IConsole console)
            : base(console, fileSystem)
        {
            Guard.Argument(faceClient, nameof(faceClient)).NotNull();
            this.faceClient = faceClient;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public override async Task ExecuteAsync()
        {
            await this.ExecuteAsync(this as ICreateFaceListArgs);
        }

        public override async Task ExecuteAsync(ICreateFaceListArgs args)
        {
            await this.faceClient.FaceList.CreateAsync(args.Id, args.Name);
            this.Console.WriteVerbose($"Successfully created FaceList with id '{args.Id}'");
        }
    }
}
