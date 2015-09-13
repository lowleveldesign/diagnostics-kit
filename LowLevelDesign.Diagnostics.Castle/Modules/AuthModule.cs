using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class AuthModule : NancyModule
    {
        public AuthModule()
        {
            Get["/auth", true] = async (x, ct) => {
                throw new NotImplementedException();
            };
        }
    }
}