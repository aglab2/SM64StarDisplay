using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class UpdateManagerHash : UpdateManager
    {
        public UpdateManagerHash() : base("StarDisplayLayouts", "layouts", "layout/updateinfo.cfg")
        {
        }

        public override string GetData()
        {
            return task.Result.Commit.Sha;
        }
    }
}
