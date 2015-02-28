using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.Connectors
{
    public sealed class HttpCastleConnector : IDisposable
    {
        private String diagnosticsMasterIP;
        private String host;

        public HttpCastleConnector(Uri url) {

        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
