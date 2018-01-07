using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class SyncManager : CachedManager
    {
        string url;
        public byte[] Data;

        const string login = "sd/login";
        string longpollStars;
        string longpollStarsOverride;
        const string post = "sd/post";

        UInt64 timestamp = 0;

        Cookie token;
        
        public SyncManager(string url, string passwd, byte[] data)
        {
            longpollStars = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("10"), Uri.EscapeDataString("stars"));
            longpollStarsOverride = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("10"), Uri.EscapeDataString("overridestars"));

            this.url = url;

            CookieAwareWebClient tokenClient;
            tokenClient = new CookieAwareWebClient();

            Data = new byte[32];
            Array.Copy(data, Data, 32);

            Auth(passwd, tokenClient);

            token = tokenClient.ResponseCookies["Auth"];
            if (token == null)
            {
                throw new ArgumentException("Failed to login!");
            }

            WebClient starsPollClient = new WebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars));

            //Pull all information from server
            SendData(Data);

            /*WebClient overrideStarsPollClient = new WebClient();
            overrideStarsPollClient.DownloadDataCompleted += starsOverridePollHandler;
            overrideStarsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            overrideStarsPollClient.DownloadDataAsync(new Uri(url + longpollStarsOverride));*/
        }

        void starsPollHandler(object sender, DownloadDataCompletedEventArgs args)
        {
            try
            {
                string jsonString = Encoding.UTF8.GetString(args.Result);

                JToken value;
                JObject o = JObject.Parse(jsonString);
                if (o.TryGetValue("timeout", out value))
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
                        Console.WriteLine(ev);
                        byte[] newData = Convert.FromBase64String(ev["data"].Value<string>());

                        if (newData.Count() != 32)
                            throw new Exception("Wrong data size");

                        UInt64.TryParse(ev["timestamp"].Value<string>(), out timestamp);

                        bool shouldSendHelp = false;
                        for (int i = 0; i < newData.Count(); i++)
                        {
                            byte diff = (byte) (Data[i] ^ newData[i]);
                            if ((Data[i] & diff) != 0)
                                shouldSendHelp = true;

                            Data[i] = (byte) (Data[i] | newData[i]);
                        }
                        isInvalidated = true;

                        if (shouldSendHelp)
                        {
                            SendData(Data);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error!: {0}", e));
            }

            WebClient starsPollClient = new WebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars + "&since_time=" + timestamp));
        }

        void starsOverridePollHandler(object sender, DownloadDataCompletedEventArgs args)
        {
            string jsonString = Encoding.UTF8.GetString(args.Result);

            JToken value;
            JObject o = JObject.Parse(jsonString);
            if (o.TryGetValue("timeout", out value))
            {
                //timeout occured
            }

            if (o.TryGetValue("error", out value))
            {
                Console.WriteLine(String.Format("Error occured: {0}", value));
            }

            if (o.TryGetValue("data", out value))
            {
                Console.WriteLine(String.Format("Data: {0}", value));
            }

            WebClient overrideStarsPollClient = new WebClient();
            overrideStarsPollClient.DownloadDataCompleted += starsOverridePollHandler;
            overrideStarsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            overrideStarsPollClient.DownloadDataAsync(new Uri(url + longpollStarsOverride));
        }

        void Auth(string passwd, CookieAwareWebClient webClient)
        {
            NameValueCollection values = new NameValueCollection();
            values.Add("password", passwd);

            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] result = webClient.UploadValues(url + login, "POST", values);
        }

        public void SendData(byte[] data)
        {
            byte[] stars = (byte[])data.Clone();
            for (int i = 0; i < 32; i += 4)
                Array.Reverse(stars, i, 4);

            string sendString = Convert.ToBase64String(data);

            WebClient postStarsClient = new WebClient();

            NameValueCollection values = new NameValueCollection();
            values.Add("stars", sendString);

            postStarsClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            postStarsClient.UploadValuesAsync(new Uri(url + post), "POST", values);
        }
    }
}
