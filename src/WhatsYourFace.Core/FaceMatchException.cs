// <copyright file="FaceMatchException.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using System.Runtime.Serialization;

#pragma warning disable CA1032 // Implement standard exception constructors; Codes are mandatory
    public class FaceMatchException : Exception
    {
        public FaceMatchException(Code errorCode)
        {
            this.ErrorCode = errorCode;
        }

        public FaceMatchException(Code errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public FaceMatchException(Code errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        protected FaceMatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public enum Code
        {
            ZeroFacesInPhoto,
            MoreThanOneFaceInPhoto,
        }

        public Code ErrorCode { get; }
    }
}
#pragma warning restore CA1032 // Implement standard exception constructors
