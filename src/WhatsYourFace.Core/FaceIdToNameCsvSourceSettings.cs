// <copyright file="FaceIdToNameCsvSourceSettings.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    public class FaceIdToNameCsvSourceSettings
    {
        public string CsvFilePath { get; set; }

        public int FaceIdIndex { get; set; }

        public int NameIndex { get; set; }

        public bool HasHeaderRow { get; set; }
    }
}
