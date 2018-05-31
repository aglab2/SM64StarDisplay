using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class UpdateManagerMsg : UpdateManager
    {
        public UpdateManagerMsg() : base("aglab2", "SM64StarDisplay", "updateinfo.cfg")
        {
        }

        public override string GetData()
        {
            return task.Result.Commit.Message;
        }
    }
}
