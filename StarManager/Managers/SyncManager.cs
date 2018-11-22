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

        public byte[] Data;

        Cookie token;
        WebClient starsPollClient;
        public bool isClosed;
        
        public SyncManager(string url, string passwd, byte[] data)
        {
            Data = new byte[32];
            if (data != null && data.Count() == 32)
                Array.Copy(data, Data, 32);

            longpollStars = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("10"), Uri.EscapeDataString("stars"));

            this.url = url;

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
            SendData(Data);

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
                            byte[] newData = Convert.FromBase64String(ev["data"].Value<string>());

                            if (newData.Count() != 32)
                                throw new Exception("Wrong data size");

                            UInt64.TryParse(ev["timestamp"].Value<string>(), out timestamp);

                            bool shouldSendHelp = false;
                            for (int i = 0; i < newData.Count(); i++)
                            {
                                byte diff = (byte)(Data[i] ^ newData[i]);
                                if ((Data[i] & diff) != 0)
                                    shouldSendHelp = true;

                                Data[i] = (byte)(Data[i] | newData[i]);
                            }
                            isInvalidated = true;

                            if (shouldSendHelp)
                            {
                                SendData(Data);
                            }
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
