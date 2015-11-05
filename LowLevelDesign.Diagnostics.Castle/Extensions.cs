using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity;
using Nancy;
using Nancy.Helpers;
using Nancy.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle
{
    public static class Extensions
    {
        public static string ReturnIfOdd(int i, string str)
        {
            return i % 2 != 0 ? str : string.Empty;
        }

        public static string GetBootstrapClassForLevel(this LogRecord logrec)
        {
            var level = logrec.LogLevel;
            switch (level) {
                case LogRecord.ELogLevel.Warning:
                    return "warning";
                case LogRecord.ELogLevel.Error:
                case LogRecord.ELogLevel.Critical:
                    return "danger";
                case LogRecord.ELogLevel.Info:
                    return "info";
                default:
                    return "default";
            }
        }

        public static string GetLogMessage(this LogRecord logrec)
        {
            if (!string.IsNullOrEmpty(logrec.Message)) {
                return logrec.Message;
            }
            if (string.Equals("IISLog", logrec.LoggerName, StringComparison.Ordinal) 
                && logrec.AdditionalFields != null) {
                object httpStatus;
                logrec.AdditionalFields.TryGetValue("HttpStatusCode", out httpStatus);
                object url;
                logrec.AdditionalFields.TryGetValue("Url", out url);
                return string.Format("HTTP: {0}, url: {1}", httpStatus, url);
            }
            return string.Empty;
        }

        public static string GetCounterValueIfAvailable(this LogRecord logrec, string counter)
        {
            var data = logrec.PerformanceData;
            float res;
            if (data == null || !data.TryGetValue(counter, out res)) {
                return string.Empty; 
            }
            if (string.Equals("CPU", counter, StringComparison.Ordinal)) {
                return res.ToString("#,0") + "% CPU";
            }
            if (string.Equals("Memory", counter, StringComparison.Ordinal)) {
                res /= 1024 * 1024; // MB
                return res.ToString("#,0.00") + "MB";
            }
            return res.ToString("#,0.00");
        }

        /// <summary>
        /// Replaces a parameter in the query string with a new value. If it does not exist
        /// it will be added.
        /// </summary>
        public static string ReplaceQueryParameterValue(this Url url, string pname, Object pvalue)
        {
            var query = url.Query;
            if (query == null) {
                return string.Format("?{0}={1}", pname, pvalue);
            }
            if (pvalue == null) {
                pvalue = string.Empty;
            }
            var parsedQuery = HttpUtility.ParseQueryString(query);
            parsedQuery.Set(pname, pvalue.ToString());
            var queryString = new StringBuilder("?");
            for (int i = 0; i < parsedQuery.Count; i++) {
                if (i > 0) {
                    queryString.Append("&");
                }
                queryString.AppendFormat("{0}={1}", parsedQuery.GetKey(i), HttpUtility.UrlEncode(parsedQuery.GetValues(i).First()));
            }
            return queryString.ToString();
        }

        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this User user, ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public static T GetFromOwinContext<T>(this NancyContext context)
        {
            return (T)context.GetOwinEnvironment()["AspNet.Identity.Owin:" + 
                    typeof(T).AssemblyQualifiedName]; // get by recompiling Microsoft.Aspnet.Identity.Owin
        }

        public static T GetFromOwinContext<T>(this NancyContext context, string typeName)
        {
            return (T)context.GetOwinEnvironment()["AspNet.Identity.Owin:" + typeName]; // get by recompiling Microsoft.Aspnet.Identity.Owin
        }

        public static bool HasPerformanceStats(this LogRecord logrec)
        {
            return logrec.PerformanceData != null && logrec.PerformanceData.Count > 0;
        }

        public static bool HasExceptionInformation(this LogRecord logrec)
        {
            return logrec.ExceptionType != null || logrec.ExceptionMessage != null;
        }
    }
}