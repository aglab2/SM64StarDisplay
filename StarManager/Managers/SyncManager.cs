using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class SyncManager
    {
        string url;

        const string login = "sd/login";
        const string longpoll = "sd/longpoll";
        const string post = "sd/post";

        CookieAwareWebClient webClient;

        public SyncManager(string url, string passwd)
        {
            this.url = url;
            webClient = new CookieAwareWebClient();
            Auth(passwd);

            Cookie token = webClient.ResponseCookies["Auth"];
            if (token == null)
            {
                throw new ArgumentException("Failed to login!");
            }
        }

        bool Auth(string passwd)
        {
            NameValueCollection values = new NameValueCollection();
            values.Add("password", passwd);

            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] result = webClient.UploadValues(url, "POST", values);
            
            string ResultAuthTicket = Encoding.UTF8.GetString(result);

            return true;
        }
    }
}
