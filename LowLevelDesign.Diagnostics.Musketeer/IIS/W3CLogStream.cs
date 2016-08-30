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

using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LowLevelDesign.Diagnostics.Musketeer.IIS
{
    /// <summary>
    /// Code is based on Tx.Windows from tx.codeplex.com
    /// </summary>
    public class W3CLogStream : IDisposable
    {
        class OpenedLogFile : IDisposable
        {
            private readonly FileStream binaryStream;
            private readonly StreamReader reader;
            private readonly string filePath;

            public OpenedLogFile(string filePath)
            {
                this.filePath = filePath;
                binaryStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                // and start the reader
                reader = new StreamReader(binaryStream);
            }

            public void SeekToTheEndOfStream()
            {
                binaryStream.Seek(0, SeekOrigin.End);
                reader.DiscardBufferedData();
            }

            public string ReadLine()
            {
                return reader.ReadLine();
            }

            public string Path
            {
                get { return filePath; }
            }

            public void Dispose()
            {
                reader.Dispose();
                binaryStream.Dispose();
            }
        }

        private static W3CEvent[] EmptyArray = new W3CEvent[0];
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string logsFolderPath;

        // next three variables define an opened file
        private OpenedLogFile currentLogFile;

        private Func<string[], W3CEvent> transform;

        public W3CLogStream(String logsFolderPath)
        {
            this.logsFolderPath = logsFolderPath;
        }

        public string CurrentlyProcessedLogFilePath
        {
            get { return currentLogFile != null ? currentLogFile.Path : null; }
        }

        public string LogsFolderPath { get { return logsFolderPath; } }

        public IEnumerable<W3CEvent> ReadLogRecords()
        {
            try
            {
                var res = new List<W3CEvent>();
                // find last log
                var newestFilePath = Directory.GetFiles(Environment.ExpandEnvironmentVariables(logsFolderPath), "*.log")
                    .OrderByDescending(s => s).FirstOrDefault();
                if (newestFilePath == null)
                {
                    logger.Info("No new log file found - closing the old one: '{0}'", currentLogFile.Path);
                    if (currentLogFile != null)
                    {
                        res.AddRange(ReadToEndOfStream());
                        currentLogFile.Dispose();
                        currentLogFile = null;
                    }
                    return res;
                }
                if (currentLogFile == null)
                {
                    logger.Info("A new log file found to monitor: '{0}'", newestFilePath);
                    currentLogFile = new OpenedLogFile(newestFilePath);

                    FindAndParseLogFileHeader();
                    currentLogFile.SeekToTheEndOfStream();
                    return EmptyArray;
                }
                if (!string.Equals(currentLogFile.Path, newestFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info("A newer log file found: '{0}', closing the old one: '{1}'", newestFilePath, 
                        currentLogFile.Path);
                    res.AddRange(ReadToEndOfStream());
                    currentLogFile.Dispose();
                    currentLogFile = new OpenedLogFile(newestFilePath);

                    // get info from the header
                    FindAndParseLogFileHeader();
                }
                res.AddRange(ReadToEndOfStream());
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured while reading logs from the log file: '{0}' - restarting", 
                    currentLogFile != null ? currentLogFile.Path : null);
                if (currentLogFile != null)
                {
                    currentLogFile.Dispose();
                    currentLogFile = null;
                }

                return EmptyArray;
            }
        }


        public void Dispose()
        {
            if (currentLogFile != null)
            {
                currentLogFile.Dispose();
            }
        }

        private bool FindAndParseLogFileHeader()
        {
            transform = null;
            Expression<Func<string[], W3CEvent>> transformExpression;
            for (;;)
            {
                string line = currentLogFile.ReadLine();
                if (line == null)
                    break;

                if (line.StartsWith("#Fields:"))
                {
                    transformExpression = GetTransformExpression(line);
                    transform = transformExpression.Compile();
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<W3CEvent> ReadToEndOfStream()
        {
            Debug.Assert(currentLogFile != null);
            if (transform == null)
            {
                yield break;
            }

            for (;;)
            {
                string line = currentLogFile.ReadLine();
                if (line == null)
                    yield break;

                if (line.StartsWith("#"))
                    continue;

                string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tokens.Length; i++)
                    if (tokens[i] == "-")
                        tokens[i] = null;

                W3CEvent e = transform(tokens);

                yield return e;
            }
        }

        static Expression<Func<string[], W3CEvent>> GetTransformExpression(string fieldsHeader)
        {
            Expression<Func<string[], W3CEvent>> template = (tok) => new W3CEvent { c_ip = tok[8] };
            LambdaExpression ex = template;
            var mi = (MemberInitExpression)ex.Body;
            var bindings = new List<MemberBinding>();
            ParameterExpression args = ex.Parameters[0];

            string[] tokens = fieldsHeader.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int dateIndex = 0;
            int timeIndex = 0;

            for (int i = 1; i < tokens.Length; i++)
            {
                string property = MakeIdentifier(tokens[i]);

                if (property == "date")
                {
                    dateIndex = i - 1;
                    continue;
                }

                if (property == "time")
                {
                    timeIndex = i - 1;
                    continue;
                }

                MemberBinding b = Expression.Bind(
                    typeof(W3CEvent).GetProperty(property),
                    Expression.ArrayIndex(args, Expression.Constant(i - 1)));

                bindings.Add(b);
            }

            MemberBinding bdt = Expression.Bind(
                typeof(W3CEvent).GetProperty("dateTime"),
                Expression.Call(
                    null,
                    typeof(W3CLogStream).GetMethod("ParseDateTime", BindingFlags.NonPublic | BindingFlags.Static),
                    Expression.ArrayIndex(args, Expression.Constant(dateIndex)),
                    Expression.ArrayIndex(args, Expression.Constant(timeIndex))));

            bindings.Add(bdt);

            NewExpression n = Expression.New(typeof(W3CEvent));
            MemberInitExpression m = Expression.MemberInit(n, bindings.ToArray());
            Expression<Func<string[], W3CEvent>> exp = Expression.Lambda<Func<string[], W3CEvent>>(m, ex.Parameters);

            return exp;
        }

        static string MakeIdentifier(string s)
        {
            return s.Replace('-', '_')
                .Replace('(', '_').Replace(')', '_')
                .Trim('_');
        }

        static DateTime ParseDateTime(string date, string time)
        {
            DateTime dt = DateTime.Parse(date + " " + time);
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
    }
}
