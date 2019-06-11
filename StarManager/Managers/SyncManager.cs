using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace StarDisplay
{
    public class SyncManager : CachedManager
    {
        string url;

        const string login = "sd/login";
        string longpollStars;
        const string post = "sd/post";

        UInt64 timestamp = 0;

        Cookie token;
        WebClient starsPollClient;
        public bool isClosed;

        public byte[] AcquiredData;
        
        public SyncManager(string url, string passwd, byte[] data)
        {
            longpollStars = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("10"), Uri.EscapeDataString("stars"));

            this.url = url;
            AcquiredData = data;

            CookieAwareWebClient tokenClient;
            tokenClient = new CookieAwareWebClient();

            Auth(passwd, tokenClient);

            token = tokenClient.ResponseCookies["Auth"];
            if (token == null)
            {
                throw new ArgumentException("Failed to login!");
            }

            starsPollClient = new WebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars));

            isClosed = false;
            //Pull all information from server
            SendData(data);

            /*WebClient overrideStarsPollClient = new WebClient();
            overrideStarsPollClient.DownloadDataCompleted += starsOverridePollHandler;
            overrideStarsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            overrideStarsPollClient.DownloadDataAsync(new Uri(url + longpollStarsOverride));*/
        }

        void starsPollHandler(object sender, DownloadDataCompletedEventArgs args)
        {
            if (isClosed)
                return;

            try
            {
                string jsonString = Encoding.UTF8.GetString(args.Result);

                JObject o = JObject.Parse(jsonString);
                if (o.TryGetValue("timeout", out JToken value))
                {
                    //timeout occured
                }

                if (o.TryGetValue("error", out value))
                {
                    Console.WriteLine(String.Format("Error occured: {0}", value));
                }

                if (o.TryGetValue("events", out value))
                {
                    Console.WriteLine(String.Format("Data: {0}", value));

                    foreach (var ev in value)
                    {
                        try
                        {
                            Console.WriteLine(ev);
                            AcquiredData = Convert.FromBase64String(ev["data"].Value<string>());

                            UInt64.TryParse(ev["timestamp"].Value<string>(), out timestamp);

                            isInvalidated = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(String.Format("Error!: {0}", e));
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error!: {0}", e));
            }

            RegisterListener(timestamp);
        }

        void Auth(string passwd, CookieAwareWebClient webClient)
        {
            NameValueCollection values = new NameValueCollection
            {
                { "password", passwd }
            };

            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] result = webClient.UploadValues(url + login, "POST", values);
        }
        
        public void RegisterListener()
        {
            starsPollClient = new WebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars));
        }

        public void RegisterListener(ulong timestamp)
        {
            starsPollClient = new WebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars + "&since_time=" + timestamp));
        }

        public void SendData(byte[] data)
        {
            WebClient postStarsClient = new WebClient();
            
            string sendString = Convert.ToBase64String(data);

            NameValueCollection values = new NameValueCollection
            {
                { "stars", sendString }
            };

            postStarsClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            postStarsClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            postStarsClient.UploadValuesAsync(new Uri(url + post), "POST", values);
        }
    }
}
