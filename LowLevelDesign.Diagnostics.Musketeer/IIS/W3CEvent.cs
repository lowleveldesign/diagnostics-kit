﻿/**
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

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace LowLevelDesign.Diagnostics.Musketeer.IIS
{
    public class W3CEvent
    {
        public DateTime dateTime { get; set; }
        public string c_ip { get; set; }
        public string cs_bytes { get; set; }
        public string cs_Cookie { get; set; }
        public string cs_host { get; set; }
        public string cs_method { get; set; }
        public string cs_Referer { get; set; }
        public string cs_uri_query { get; set; }
        public string cs_uri_stem { get; set; }
        public string cs_User_Agent { get; set; }
        public string cs_username { get; set; }
        public string cs_version { get; set; }
        public string s_computername { get; set; }
        public string s_ip { get; set; }
        public string s_port { get; set; }
        public string s_sitename { get; set; }
        public string sc_bytes { get; set; }
        public string sc_status { get; set; }
        public string sc_substatus { get; set; }
        public string sc_win32_status { get; set; }
        public string time_taken { get; set; }
    }
}
