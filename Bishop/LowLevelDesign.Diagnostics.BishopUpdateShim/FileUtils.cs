/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LowLevelDesign.Diagnostics.BishopUpdateShim
{
    static class FileUtils
    {
        public static void ExtractZipToDirectoryAndOverrideExistingFiles(string zipFile, string destinationDirectoryName)
        {
            using (var source = ZipFile.OpenRead(zipFile))
            {
                string fullName = Directory.CreateDirectory(destinationDirectoryName).FullName;
                foreach (ZipArchiveEntry current in source.Entries)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(fullName, current.FullName));
                    if (!fullPath.StartsWith(fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Path outside the destination folder");
                    }
                    if (Path.GetFileName(fullPath).Length == 0)
                    {
                        if (current.Length != 0L)
                        {
                            throw new IOException("Directory Name With Data");
                        }
                        Directory.CreateDirectory(fullPath);
                    }
                    else {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        current.ExtractToFile(fullPath, true);
                    }
                }
            }
        }

        public static string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public static void CreateZipFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, string[] exclusionList)
        {
            if (exclusionList == null)
            {
                exclusionList = new string[0];
            }
            sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);
            destinationArchiveFileName = Path.GetFullPath(destinationArchiveFileName);
            using (ZipArchive zipArchive = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Create, Encoding.UTF8))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirectoryName);
                string fullName = directoryInfo.FullName;
                foreach (FileSystemInfo current in directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    int length = current.FullName.Length - fullName.Length;
                    string text = current.FullName.Substring(fullName.Length, length);
                    text = text.TrimStart(new char[]
                    {
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    });
                    if (current is FileInfo)
                    {
                        if (!exclusionList.Contains(current.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            CreateZipArchiveEntryFromFile(zipArchive, current.FullName, text);
                        }
                    }
                }
            }
        }

        private static ZipArchiveEntry CreateZipArchiveEntryFromFile(ZipArchive destination, string sourceFileName, string entryName)
        {
            ZipArchiveEntry result;
            using (Stream stream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ZipArchiveEntry zipArchiveEntry = destination.CreateEntry(entryName);
                DateTime lastWriteTime = File.GetLastWriteTime(sourceFileName);
                if (lastWriteTime.Year < 1980 || lastWriteTime.Year > 2107)
                {
                    lastWriteTime = new DateTime(1980, 1, 1, 0, 0, 0);
                }
                zipArchiveEntry.LastWriteTime = lastWriteTime;
                using (Stream stream2 = zipArchiveEntry.Open())
                {
                    stream.CopyTo(stream2);
                }
                result = zipArchiveEntry;
            }
            return result;
        }
    }
}
