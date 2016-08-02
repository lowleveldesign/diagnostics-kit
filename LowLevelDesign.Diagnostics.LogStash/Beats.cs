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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace LowLevelDesign.Diagnostics.LogStash
{
    public class Beats : IDisposable
    {
        private const int WindowSize = 10;

        private readonly Queue<string> serializedEventsQueue = new Queue<string>(WindowSize);

        // FIXME support SSL
        private TcpClient logStashTcpClient;

        public Beats(string logStashServer, int logStashPort)
        {
            logStashTcpClient = new TcpClient(logStashServer, logStashPort);
        }

        public void SendEvent(string beat, string type, DateTime timestampUtc, Dictionary<string, object> eventData)
        {
            eventData.Add("@metadata", new Dictionary<string, string> {
                { "type", type },
                { "beat", beat }
            });
            eventData.Add("@timestamp", timestampUtc);
            eventData.Add("type", type);

            var serializedEventData = JsonConvert.SerializeObject(eventData, Formatting.None);
            serializedEventsQueue.Enqueue(serializedEventData);
            if (serializedEventsQueue.Count == WindowSize) {
                // we need to send the collected events
                ProcessEventsQueue(serializedEventsQueue.ToArray());
                serializedEventsQueue.Clear();
            }
        }

        private void ProcessEventsQueue(string[] events)
        {
            if (events == null || events.Length == 0) {
                return;
            }

            // FIXME: should be async, we should always try to restore the connection
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

            var stream = logStashTcpClient.GetStream();
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(stream);

            // there should be one ACK message on the output
            int ackseq = 0;
            while (seq != ackseq) {
                byte[] buffer = new byte[6]; // ACK message is: 2 A <4-byte-seq-number> 
                stream.Read(buffer, 0, buffer.Length);
                ackseq = BigEndianBitConverter.ToInt32(buffer, 2);
            }
        }

        public void Dispose()
        {
            // send all pending events
            if (serializedEventsQueue.Count > 0) {
                ProcessEventsQueue(serializedEventsQueue.ToArray());
                serializedEventsQueue.Clear();
            }

            logStashTcpClient.Close();
        }
    }
}
