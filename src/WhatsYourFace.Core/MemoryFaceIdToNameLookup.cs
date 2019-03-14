// <copyright file="MemoryFaceIdToNameLookup.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Dawn;
    using WhatsYourFace.Models;

    public class MemoryFaceIdToNameLookup : IFaceIdToNameLookup
    {
        private readonly Dictionary<Guid, string> dictionary = new Dictionary<Guid, string>();

        private MemoryFaceIdToNameLookup()
        {
        }

        public static MemoryFaceIdToNameLookup FromCsvFile(CsvSettings settings)
        {
            Guard.Argument(settings, nameof(settings)).NotNull();
            Guard.Argument(settings.CsvFilePath, nameof(settings.CsvFilePath)).NotWhiteSpace();
            Guard.Argument(settings.FaceIdIndex, nameof(settings.FaceIdIndex)).NotNegative();
            Guard.Argument(settings.FaceIdIndex, nameof(settings.NameIndex)).NotNegative();

            using (TextReader reader = File.OpenText(settings.CsvFilePath))
            {
                return FromCsvStream(reader, settings);
            }
        }

        public static MemoryFaceIdToNameLookup FromCsvStream(TextReader csvReader, CsvSettings settings)
        {
            Guard.Argument(csvReader, nameof(csvReader)).NotNull();
            Guard.Argument(settings, nameof(settings)).NotNull();
            Guard.Argument(settings.FaceIdIndex, nameof(settings.FaceIdIndex)).NotNegative();
            Guard.Argument(settings.FaceIdIndex, nameof(settings.NameIndex)).NotNegative();

            SkipHeaderRow(csvReader, settings);

            string csvLine;
            var result = new MemoryFaceIdToNameLookup();

            while ((csvLine = csvReader.ReadLine()) != null)
            {
                AddLineToResult(csvLine, result, settings);
            }

            return result;
        }

        public string LookupName(Guid persistedFaceId, FaceCategory category)
        {
            // Note (vladcananau): at the moment the category is not used
            // because we have a small dataset all in one file; but for scaling
            // up it makes sense to be here;
            return this.dictionary[persistedFaceId];
        }

        private static void AddLineToResult(string csvLine, MemoryFaceIdToNameLookup result, CsvSettings settings)
        {
            string[] entry = csvLine.Split(',');

            if (entry.Length < GetExpectedColumns(settings))
            {
                string message = $"{settings.CsvFilePath} (line {GetCurrentLine(result, settings)}): "
                    + " Expected {GetExpectedColumns(settings)} fields but found only {entry.Length}.";
                throw new ArgumentOutOfRangeException(nameof(settings), message);
            }

            result.dictionary.Add(Guid.Parse(entry[settings.FaceIdIndex]), entry[settings.NameIndex]);
        }

        private static int GetCurrentLine(MemoryFaceIdToNameLookup result, CsvSettings settings)
        {
            return result.dictionary.Count + (settings.HasHeaderRow ? 1 : 0);
        }

        private static int GetExpectedColumns(CsvSettings settings)
        {
            return Math.Max(settings.FaceIdIndex, settings.NameIndex) + 1; // +1 because zero-index
        }

        private static void SkipHeaderRow(TextReader csvReader, CsvSettings settings)
        {
            if (settings.HasHeaderRow)
            {
                csvReader.ReadLine();
            }
        }

        public class CsvSettings
        {
            public string CsvFilePath { get; set; }

            public int FaceIdIndex { get; set; }

            public int NameIndex { get; set; }

            public bool HasHeaderRow { get; set; }
        }
    }
}
