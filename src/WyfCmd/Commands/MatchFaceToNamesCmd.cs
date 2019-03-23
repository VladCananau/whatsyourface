// <copyright file="MatchFaceToNamesCmd.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using Dawn;
    using WhatsYourFace.Core;
    using WhatsYourFace.Models;

    [Verb("whatsyourface", HelpText = "Matches a photo of your face to the names that it most looks like.")]
    public class MatchFaceToNamesCmd : CmdBase<IMatchFaceToNamesArgs, FaceToNameMatchResult>, IMatchFaceToNamesArgs
    {
        private readonly IFaceMatcher faceMatcher;

        public MatchFaceToNamesCmd()
        {
        }

        public MatchFaceToNamesCmd(IFaceMatcher faceMatcher, IFileSystem fileSystem, IConsole console)
            : base(console, fileSystem)
        {
            Guard.Argument(faceMatcher, nameof(faceMatcher)).NotNull();
            this.faceMatcher = faceMatcher;
        }

        public string ImageFilePath { get; set; }

        public string CountryCode { get; set; }

        public int MaxCandidateFaces { get; set; }

        public ResultAggregation Aggregation { get; set; }

        public override async Task<FaceToNameMatchResult> ExecuteAsync()
        {
            return await this.ExecuteAsync(this as IMatchFaceToNamesArgs);
        }

        public override async Task<FaceToNameMatchResult> ExecuteAsync(IMatchFaceToNamesArgs args)
        {
            FaceToNameMatchResult result;
            using (FileStream stream = File.OpenRead(args.ImageFilePath))
            {
                result = await this.faceMatcher.MatchFaceToNameAsync(stream, args.CountryCode, args.MaxCandidateFaces);
            }

            return OrderMatches(AggregateMatches(result, args.Aggregation));
        }

        private static FaceToNameMatchResult OrderMatches(FaceToNameMatchResult result)
        {
            return new FaceToNameMatchResult(
                result.Category,
                result.Matches.OrderByDescending(_ => _.Score));
        }

        private static FaceToNameMatchResult AggregateMatches(FaceToNameMatchResult result, ResultAggregation aggregation)
        {
            if (aggregation == ResultAggregation.None)
            {
                return result;
            }

            IEnumerable<FaceToNameMatch> aggregated;

            if (aggregation == ResultAggregation.Count)
            {
                aggregated = result.Matches.CountMatchesByName();
            }
            else if (aggregation == ResultAggregation.Sum)
            {
                aggregated = result.Matches.SumScoresByName();
            }
            else
            {
                throw new NotSupportedException($"Unsupported aggregation '{aggregation}'");
            }

            return new FaceToNameMatchResult(result.Category, aggregated);
        }
    }
}
