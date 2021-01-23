using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay.Server
{
    public class ExtendedWebClient : WebClient
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent connections (default is 2).
        /// </summary>
        public int ConnectionLimit { get; set; }

        /// <summary>
        /// Creates a new instance of ExtendedWebClient.
        /// </summary>
        public ExtendedWebClient()
        {
            this.ConnectionLimit = 20;
        }

        /// <summary>
        /// Creates the request for this client and sets connection defaults.
        /// </summary>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null)
            {
                request.ServicePoint.ConnectionLimit = this.ConnectionLimit;
            }

            return request;
        }
    }
}
