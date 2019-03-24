// <copyright file="FileSystemExtensions.cs" company="Vlad Ionut Cananau">
// Copyright (c) Vlad Ionut Cananau. All rights reserved.
// </copyright>

namespace WhatsYourFace.DataSet
{
    using System;
    using System.IO;
    using System.IO.Abstractions;

    public static class FileSystemExtensions
    {
        /// <summary>
        /// Creates and deletes a file from an existing directory on disk.
        /// </summary>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> instance being extended.</param>
        /// <param name="dirPath">The directory path.</param>
        /// <param name="throwOnFailure">
        /// If <c>true</c>, an exception will be thrown (allowed to bubble) when the operation fails.
        /// Otherwise, the method will simply return <c>false</c>. Default is <c>false</c>.
        /// </param>
        /// <returns>true, if the operation was successful, false otherwise.</returns>
        public static bool TryCreateDeleteTempFileToDirectory(this IFileSystem fileSystem, string dirPath, bool throwOnFailure = false)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                throw new ArgumentException("Directory path cannot be null, empty or whitespace", nameof(dirPath));
            }

            if (!fileSystem.Directory.Exists(dirPath))
            {
                throw new ArgumentException($"Directory does not exist: {dirPath}", nameof(dirPath));
            }

            string tempfile = fileSystem.Path.Combine(dirPath, fileSystem.Path.GetRandomFileName());

            try
            {
                using (Stream fs = fileSystem.File.Create(tempfile, 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch
            {
                if (throwOnFailure)
                {
                    throw;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void TestWritePermissionsOrCreateDirectoryIfNotExists(this IFileSystem fileSystem, string directory)
        {
            if (Directory.Exists(directory))
            {
                // Check write permissions early to avoid making needless network calls if we can't save
                fileSystem.TryCreateDeleteTempFileToDirectory(directory, throwOnFailure: true);
            }
            else
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
