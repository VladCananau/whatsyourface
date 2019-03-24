// <copyright file="FaceMatchException.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.Core
{
    using System;
    using System.Runtime.Serialization;

    public class FaceMatchException : Exception
    {
        public FaceMatchException(Codes errorCode)
        {
            this.ErrorCode = errorCode;
        }

        public FaceMatchException(Codes errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public FaceMatchException(Codes errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        protected FaceMatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public enum Codes
        {
            ZeroFacesInPhoto,
            MoreThanOneFaceInPhoto,
        }

        public Codes ErrorCode { get; }
    }
}
