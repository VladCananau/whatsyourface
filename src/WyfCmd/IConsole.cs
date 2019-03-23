// <copyright file="IConsole.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace WhatsYourFace.Wyfcmd
{
    using System;
    using WhatsYourFace.Models;

    public interface IConsole
    {
        void WriteInformation(string text);

        void WriteVerbose(string text);

        void WriteWarning(string text);

        void WriteObject(FaceToNameMatchResult result);
    }

    public class CommandPrompt : IConsole
    {
        public void WriteInformation(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteVerbose(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteWarning(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteObject(FaceToNameMatchResult result)
        {
            Console.WriteLine($"{result.Category.CountryCode}, {result.Category.Gender}");

            foreach (FaceToNameMatch score in result.Matches)
            {
                Console.WriteLine($"{score.FirstName}\t{score.Score}");
            }
        }
    }
}