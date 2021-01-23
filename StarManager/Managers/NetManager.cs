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
        public bool mustReload = true;

        public NetManager()
        {
            watch.Start();
            deadWatch.Start();
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
                if(elapsed > 30)
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

        public List<string> GetPlayers()
        {
            return players;
        }
    }
}
