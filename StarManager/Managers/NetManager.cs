using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StarDisplay
{
    public class NetManager : CachedManager
    {
        Stopwatch watch = new Stopwatch();
        Stopwatch deadWatch = new Stopwatch();

        List<string> players = new List<string>();
        List<string> deadPlayers = new List<string>();
        UInt32 call;
        public bool mustReload;

        public NetManager()
        {
            byte[] bin = Resource.NetBin;
            byte[] func = new byte[4];
            Array.Copy(func, 0, bin, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(func);

            UInt32 addr = BitConverter.ToUInt32(func, 0);
            call = (addr / 4) | 0x0c000000;

            watch.Start();
        }

        public override bool CheckInvalidated()
        {
            bool invalidated = false;
            if (deadWatch.ElapsedMilliseconds > 30000)
            {
                if (deadPlayers.Count() != 0)
                {
                    invalidated = true;
                    mustReload = true;
                    foreach (var player in deadPlayers)
                    {
                        players.Remove(player);
                    }
                }
                deadPlayers = new List<string>(players);
                deadWatch.Restart();
            }

            {
                var elapsed = watch.ElapsedMilliseconds;
                if(elapsed > 1000)
                {
                    invalidated = true;
                    watch.Restart();
                }
            }

            return invalidated;
        }

        public int RegisterPlayer(string name)
        {
            deadPlayers.Remove(name);
            if (!players.Contains(name))
                players.Add(name);

            return players.IndexOf(name);
        }
    }
}
