using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using StarDisplay.Server;

namespace StarDisplay
{
    public struct NetPlayer
    {
        public NetPlayer(byte[] state, string location, UInt64 timestamp)
        {
            this.state = state;
            this.location = location;
            this.timestamp = timestamp;
        }

        public byte[] state;
        public string location;
        public UInt64 timestamp;
    };

    public class SyncManager : CachedManager
    {
        string url;

        const string login = "sd/login";
        string longpollStars;
        string longpollNet;
        const string post = "sd/post";

        UInt64 starsTimestamp = 0;
        UInt64 netTimestamp = 0;

        Cookie token;
        
        public bool dropFile = false;
        public bool isClosed = false;
        public bool listenNet = false;

        public byte[] AcquiredData;
        public Mutex NetDataMtx = new Mutex();

        Dictionary<string, NetPlayer> NetData;

        string category;
        
        public SyncManager(string url, string passwd, byte[] data, bool net, string category)
        {
            this.category = category;
            listenNet = net;
            longpollStars = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("10"), Uri.EscapeDataString(category));
            longpollNet = string.Format("sd/longpoll?timeout={0}&category={1}", Uri.EscapeDataString("1"), Uri.EscapeDataString("net"));

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

            RegisterStarsListener();
            RegisterNetListener();

            isClosed = false;
            //Pull all information from server
            dropFile = true;
            SendData(data);

            /*WebClient overrideStarsPollClient = new WebClient();
            overrideStarsPollClient.DownloadDataCompleted += starsOverridePollHandler;
            overrideStarsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            overrideStarsPollClient.DownloadDataAsync(new Uri(url + longpollStarsOverride));*/
        }

        ~SyncManager()
        {
            NetDataMtx.Dispose();
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
                            var data = ev["data"];
                            AcquiredData = Convert.FromBase64String(data.Value<string>());

                            UInt64 timestamp;
                            UInt64.TryParse(ev["timestamp"].Value<string>(), out timestamp);
                            if (timestamp > starsTimestamp)
                                starsTimestamp = timestamp;

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

            RegisterStarsListener(starsTimestamp);
        }

        public Dictionary<string, NetPlayer> getNetData()
        {
            Dictionary<string, NetPlayer> ptr;
            NetDataMtx.WaitOne();
            ptr = NetData;
            NetData = new Dictionary<string, NetPlayer>();
            NetDataMtx.ReleaseMutex();
            return ptr;
        }

        void netPollHandler(object sender, DownloadDataCompletedEventArgs args)
        {
            if (isClosed || !listenNet)
                return;

            NetDataMtx.WaitOne();
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
                    foreach (var ev in value)
                    {
                        try
                        {
                            var dataStr = ev["data"];

                            var data = JObject.Parse(dataStr.Value<string>());

                            var player = data["player"].Value<string>();
                            var state = data["state"].Value<string>();
                            var location = data["location"].Value<string>();

                            UInt64.TryParse(ev["timestamp"].Value<string>(), out UInt64 timestamp);
                            if (timestamp > netTimestamp)
                                netTimestamp = timestamp;

                            if (NetData.ContainsKey(player))
                            {
                                var playerData = NetData[player];
                                if (playerData.timestamp < timestamp)
                                    NetData[player] = new NetPlayer(Convert.FromBase64String(state), location, timestamp);
                            }
                            else
                            {
                                NetData[player] = new NetPlayer(Convert.FromBase64String(state), location, timestamp);
                            }

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
            finally
            {
                NetDataMtx.ReleaseMutex();
            }

            RegisterNetListener(netTimestamp);
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
        
        public void RegisterStarsListener()
        {
            var starsPollClient = new ExtendedWebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars));
        }

        public void RegisterStarsListener(ulong timestamp)
        {
            var starsPollClient = new ExtendedWebClient();
            starsPollClient.DownloadDataCompleted += starsPollHandler;
            starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            starsPollClient.DownloadDataAsync(new Uri(url + longpollStars + "&since_time=" + timestamp));
        }

        public void RegisterNetListener()
        {
            if (listenNet)
            {
                var starsPollClient = new ExtendedWebClient();
                starsPollClient.DownloadDataCompleted += netPollHandler;
                starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
                starsPollClient.DownloadDataAsync(new Uri(url + longpollNet));
            }
        }

        public void RegisterNetListener(ulong timestamp)
        {
            if (listenNet)
            {
                var starsPollClient = new ExtendedWebClient();
                starsPollClient.DownloadDataCompleted += netPollHandler;
                starsPollClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
                starsPollClient.DownloadDataAsync(new Uri(url + longpollNet + "&since_time=" + timestamp));
            }
        }

        public void SendData(byte[] data)
        {
            var postStarsClient = new ExtendedWebClient();
            
            string sendString = Convert.ToBase64String(data);

            NameValueCollection values = new NameValueCollection
            {
                { category, sendString }
            };

            postStarsClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            postStarsClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            postStarsClient.UploadValuesAsync(new Uri(url + post), "POST", values);
        }

        public void SendNet64Data(string name, byte[] data, string location)
        {
            if (!(data is object))
                return;

            var postStarsClient = new ExtendedWebClient();

            JObject json = new JObject
            {
                ["player"] = name,
                ["state"] = Convert.ToBase64String(data),
                ["location"] = location
            };

            string sendString = json.ToString();

            NameValueCollection values = new NameValueCollection
            {
                { "net", sendString }
            };

            postStarsClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            postStarsClient.Headers.Add(HttpRequestHeader.Cookie, token.ToString());
            postStarsClient.UploadValuesAsync(new Uri(url + post), "POST", values);
        }
    }
}
