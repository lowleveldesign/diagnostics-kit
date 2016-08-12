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

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LowLevelDesign.Diagnostics.LogStash
{
    public class Beats : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan MaxDelayInSendingLogs = TimeSpan.FromMinutes(3);

        private const int WindowSize = 10;

        private readonly Queue<string> serializedEventsQueue = new Queue<string>(WindowSize);
        private readonly string logStashServer;
        private readonly int logStashPort;
        private readonly bool useSsl;
        private readonly string certThumb;

        private TcpClient logStashTcpClient;
        private Stream currentStream;
        private DateTime lastTimeEventsWereSentUtc = DateTime.MinValue;

        public Beats(string logStashServer, int logStashPort, bool useSsl, string certThumb = null)
        {
            this.logStashServer = logStashServer;
            this.logStashPort = logStashPort;
            this.useSsl = useSsl;
            this.certThumb = certThumb;

            RenewConnectionStream();
        }

        private void RenewConnectionStream()
        {
            logStashTcpClient = new TcpClient(logStashServer, logStashPort);
            if (useSsl) {
                var sslStream = new SslStream(logStashTcpClient.GetStream(), false, null, 
                    !string.IsNullOrEmpty(certThumb) ? (LocalCertificateSelectionCallback)SelectLocalCertificate : null, 
                    EncryptionPolicy.RequireEncryption);
                sslStream.AuthenticateAsClient(logStashServer);
                currentStream = sslStream;
            } else {
                currentStream = logStashTcpClient.GetStream();
            }
        }

        private static X509Certificate SelectLocalCertificate(object sender, string targetHost, 
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return null;
        }

        public void SendEvent(string beat, string type, DateTime timestampUtc, Dictionary<string, object> eventData)
        {
            if (lastTimeEventsWereSentUtc == DateTime.MinValue) {
                lastTimeEventsWereSentUtc = DateTime.UtcNow;
            }

            eventData.Add("@metadata", new Dictionary<string, string> {
                { "type", type },
                { "beat", beat }
            });
            eventData.Add("@timestamp", timestampUtc);
            eventData.Add("type", type);

            var serializedEventData = JsonConvert.SerializeObject(eventData, Formatting.None);
            serializedEventsQueue.Enqueue(serializedEventData);
            if (serializedEventsQueue.Count == WindowSize || 
                DateTime.UtcNow.Subtract(lastTimeEventsWereSentUtc) > MaxDelayInSendingLogs) {
                ProcessCollectedEventsQueue();
                lastTimeEventsWereSentUtc = DateTime.UtcNow;
            }
        }

        private void ProcessCollectedEventsQueue()
        {
            var events = serializedEventsQueue.ToArray();
            if (events.Length == 0) {
                return;
            }
            serializedEventsQueue.Clear();

            if (!logStashTcpClient.Connected) {
                logStashTcpClient = new TcpClient(logStashServer, logStashPort);
            }

            var outputStream = new MemoryStream();
            outputStream.WriteAllBytes(Encoding.ASCII.GetBytes("2W"));
            outputStream.WriteAllBytes(BigEndianBitConverter.GetBytes(events.Length));

            var compressedStream = new MemoryStream();
            int seq = 0;
            var deflater = new Deflater(-1, false);
            using (var deflateStream = new DeflaterOutputStream(compressedStream, deflater)) {
                deflateStream.IsStreamOwner = false;
                foreach (var ev in events) {
                    seq += 1;

                    deflateStream.WriteAllBytes(Encoding.ASCII.GetBytes("2J"));
                    deflateStream.WriteAllBytes(BigEndianBitConverter.GetBytes(seq));

                    var encodedEvent = Encoding.UTF8.GetBytes(ev);
                    deflateStream.WriteAllBytes(BigEndianBitConverter.GetBytes(encodedEvent.Length));
                    deflateStream.WriteAllBytes(encodedEvent);
                }
            }

            outputStream.WriteAllBytes(Encoding.ASCII.GetBytes("2C"));
            outputStream.WriteAllBytes(BigEndianBitConverter.GetBytes((int)compressedStream.Length));

            compressedStream.Seek(0, SeekOrigin.Begin);
            compressedStream.CopyTo(outputStream);

            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(currentStream);
            currentStream.Flush();

            // there should be one ACK message on the output
            int ackseq = 0;
            while (seq != ackseq) {
                byte[] buffer = new byte[6]; // ACK message is: 2 A <4-byte-seq-number> 
                currentStream.Read(buffer, 0, buffer.Length);
                ackseq = BigEndianBitConverter.ToInt32(buffer, 2);
            }
        }

        public void Dispose()
        {
            // send all pending events
            if (serializedEventsQueue.Count > 0) {
                ProcessCollectedEventsQueue();
            }

            currentStream.Close();
            logStashTcpClient.Close();
        }

    }
}
