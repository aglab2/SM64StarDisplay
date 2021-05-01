using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    struct Instruction
    {
        public Cmd cmd;
        public Register? rs;
        public Register? rt;
        public Register? rd;
        public int? shift;
        public short? imm;
        public int? off;
        public uint? jump;
    }
}
