// <copyright file="FaceUploader.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFaceUploader
    {
        Task<IList<Guid>> UploadPhotosToFaceList(string directory, int count, int skip, string faceListId, string userData);
    }
}