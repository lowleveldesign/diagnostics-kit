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
using Fiddler;

namespace LowLevelDesign.Diagnostics.Bishop.Common
{
    public sealed class Request : IRequest
    {
        private readonly bool isLocal;
        private readonly bool isHttpsConnect;
        private readonly Session fiddlerSession;

        public Request(Session oSession)
        {
            isLocal = oSession.HostnameIs("localhost") || oSession.HostnameIs("127.0.0.1") || 
                oSession.HostnameIs("[::1]");
            isHttpsConnect = oSession.HTTPMethodIs("CONNECT");
            fiddlerSession = oSession;
        }

        public bool IsLocal { get { return isLocal; } }

        public bool IsHttps { get { return fiddlerSession.isHTTPS; } }

        public bool IsHttpsConnect { get { return isHttpsConnect; } }

        public string FullUrl { get { return fiddlerSession.fullUrl; } }

        public string PathAndQuery { get { return fiddlerSession.PathAndQuery; } }

        public string Host { get { return fiddlerSession.host; } }

        public int Port { get { return fiddlerSession.port; } }

        public Session FiddlerSession { get { return fiddlerSession; } }

        public string Protocol { get { return fiddlerSession.isHTTPS ? "https" : "http";  } }

        public void SetHeader(string header, string value)
        {
            fiddlerSession.oRequest.headers.Add(header, value);
        }
    }
}
