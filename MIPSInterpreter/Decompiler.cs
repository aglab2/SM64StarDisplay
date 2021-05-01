using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Decompiler
    {
        public Decompiler()
        { }

        public Instruction Decompile(uint inst)
        {
            Instruction ret = new Instruction();

            var op = (Op)Converter.Extract(inst, 26, 6);
            if (op == Op.SPECIAL)
            {
                var funct = (Funct)Converter.Extract(inst, 0, 6);
                ret.cmd = Converter.ToCmd(funct);
            }
            else if (op == Op.REGIMM)
            {
                var functImm = (FunctImm)Converter.Extract(inst, 16, 5);
                ret.cmd = Converter.ToCmd(functImm);
            }
            else
            {
                ret.cmd = Converter.ToCmd(op);
            }

            var format = Converter.ToFormat(ret.cmd);
            if (format.HasFlag(Format.IMM))
            {
                ret.imm = (short) Converter.Extract(inst, 0, 16);
            }
            if (format.HasFlag(Format.OFF))
            {
                var off = (short) Converter.Extract(inst, 0, 16);
                ret.off = Converter.ExtendSign(off) << 2;
            }
            if (format.HasFlag(Format.JUMP))
            {
                ret.jump = Converter.Extract(inst, 0, 26);
            }
            if (format.HasFlag(Format.REG_S))
            {
                ret.rs = (Register)Converter.Extract(inst, 21, 5);
            }
            if (format.HasFlag(Format.REG_T))
            {
                ret.rt = (Register)Converter.Extract(inst, 16, 5);
            }
            if (format.HasFlag(Format.REG_D))
            {
                ret.rd = (Register)Converter.Extract(inst, 11, 5);
            }
            if (format.HasFlag(Format.REG_A))
            {
                ret.shift = (ushort) Converter.Extract(inst, 6, 5);
            }
            return ret;
        }
    }
}
