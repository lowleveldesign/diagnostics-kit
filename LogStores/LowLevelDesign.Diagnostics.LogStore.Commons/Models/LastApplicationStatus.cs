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

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public class LastApplicationStatus
    {
        private static readonly TimeSpan StatusValidityPeriod = TimeSpan.FromSeconds(60);

        public string ApplicationPath { get; set; }

        public string Server { get; set; }

        public float Cpu { get; set; }

        public float Memory { get; set; }

        public DateTime? LastPerformanceDataUpdateTimeUtc { get; set; }

        public bool ContainsPerformanceData()
        {
            return LastPerformanceDataUpdateTimeUtc.HasValue && DateTime.UtcNow.Subtract(
                LastPerformanceDataUpdateTimeUtc.Value) < StatusValidityPeriod;
        }

        public string LastErrorType { get; set; }

        public DateTime? LastErrorTimeUtc { get; set; }

        public bool ContainsErrorInformation()
        {
            return LastErrorTimeUtc.HasValue && DateTime.UtcNow.Subtract(
                LastErrorTimeUtc.Value) < StatusValidityPeriod;
        }

        public DateTime LastUpdateTimeUtc { get; set; }

        public string LastErrorTypeName
        {
            get
            {
                if (LastErrorType == null) {
                    return null;
                }
                var ind = LastErrorType.LastIndexOf('.');
                return ind == -1 ? LastErrorType : LastErrorType.Substring(ind + 1);
            }
        }
    }
}
