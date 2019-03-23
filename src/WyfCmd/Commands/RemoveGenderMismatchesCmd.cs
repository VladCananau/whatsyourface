// <copyright file="RemoveGenderMismatchesCmd.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using CommandLine;
    using Dawn;
    using WhatsYourFace.DataSet;
    using WhatsYourFace.Models;

    [Verb("removemismatches", HelpText = "Removes images that contain faces of a different gender than expected.")]
    public class RemoveGenderMismatchesCmd : CmdBase<IRemoveGenderMismatchesArgs>, IRemoveGenderMismatchesArgs
    {
        private readonly IImageSanitizer sanitizer;

        public RemoveGenderMismatchesCmd()
        {
        }

        public RemoveGenderMismatchesCmd(
            IImageSanitizer sanitizer,
            IFileSystem fileSystem,
            IConsole console)
            : base(console, fileSystem)
        {
            Guard.Argument(sanitizer, nameof(sanitizer)).NotNull();
            this.sanitizer = sanitizer;
        }

        public string ImagesFolder { get; set; }

        public FaceGender Gender { get; set; }

        public string QuarantineFolder { get; set; }

        public override async Task ExecuteAsync()
        {
            await this.ExecuteAsync(this as IRemoveGenderMismatchesArgs);
        }

        public override async Task ExecuteAsync(IRemoveGenderMismatchesArgs args)
        {
            await this.sanitizer.RemoveGenderMismatches(args.ImagesFolder, args.Gender, args.QuarantineFolder);
        }
    }
}
