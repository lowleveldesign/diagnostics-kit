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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LowLevelDesign.Diagnostics.Bishop.Config
{
    public sealed class PluginSettings
    {
        [JsonProperty(PropertyName = "EncryptedPassword")]
        private byte[] encryptedPassword;

        private IEnumerable<RequestTransformation> transformations = new RequestTransformation[0];

        private IEnumerable<HttpsLocalRedirect> httpsRedirects = new HttpsLocalRedirect[0];

        public Uri DiagnosticsUrl { get; set; }

        public string UserName { get; set; }

        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) {
                encryptedPassword = null;
                return;
            }
            encryptedPassword = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null,
                DataProtectionScope.CurrentUser);
        }

        public string GetPassword()
        {
            if (encryptedPassword == null) {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedPassword,
                null, DataProtectionScope.CurrentUser));
        }

        [JsonIgnore]
        internal byte[] EncryptedPassword { get { return encryptedPassword; } }

        public IEnumerable<RequestTransformation> UserDefinedTransformations
        {
            get { return transformations; }
            set { transformations = value; }
        }

        public IEnumerable<HttpsLocalRedirect> HttpsRedirects
        {
            get { return httpsRedirects; }
            set { httpsRedirects = value; }
        }

        public void Save(string configFilePath)
        {
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(this));
        }

        public int FindLocalPortForHttpsRedirection(int remotePort)
        {
            var httpRedirect = httpsRedirects.Where(r => r.RemoteHttpsPort == remotePort).FirstOrDefault();
            return httpRedirect == null ? 0 : httpRedirect.LocalHttpPort;
        }

        public static PluginSettings Load(string configFilePath)
        {
            if (!File.Exists(configFilePath)) {
                return new PluginSettings();
            }
            return JsonConvert.DeserializeObject<PluginSettings>(File.ReadAllText(
                configFilePath)) ?? new PluginSettings();
        }
    }
}
