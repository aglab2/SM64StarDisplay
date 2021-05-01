using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Interpreter
    {
        public int[] gpr = new int[32];
        public int lo;
        public int hi;
        public uint pc;

        delegate void Performer(Instruction inst);

        readonly Decompiler decompiler = new Decompiler();
        readonly Memory memory;
        readonly Dictionary<Cmd, Performer> cmdToFunc;

        void Add(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] + gpr[rt];
        }

        void AddI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            short imm = inst.imm.Value;
            gpr[rt] = (int) gpr[rs] + imm;
        }

        void And(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] & gpr[rt];
        }

        void AndI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            short imm = inst.imm.Value;
            gpr[rt] = imm & gpr[rs];
        }

        void Div(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rsVal = gpr[rs];
            int rtVal = gpr[rt];
            lo = rsVal % rtVal;
            hi = rsVal / rtVal;
        }

        void DivU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            uint rsVal = (uint)gpr[rs];
            uint rtVal = (uint) gpr[rt];
            lo = (int) (rsVal % rtVal);
            hi = (int) (rsVal / rtVal);
        }

        void LB(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (char)(pAddr >> (24 - dataPos * 8));
        }

        void LBU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (byte) (pAddr >> (24 - dataPos * 8));
        }

        void LH(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (short)(pAddr >> (16 - shortPos * 16));
        }

        void LHU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (ushort) (pAddr >> (16 - shortPos * 16));
        }

        void LUI(Instruction inst)
        {
            int rt = (int)inst.rt.Value;
            int imm = inst.imm.Value;
            gpr[rt] = imm << 16;
        }

        void LW(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            gpr[rt] = (int) memory.Read(vAddr);
        }

        void MFHI(Instruction inst)
        {
            int rd = (int)inst.rd.Value;
            gpr[rd] = hi;
        }

        void MFLO(Instruction inst)
        {
            int rd = (int)inst.rd.Value;
            gpr[rd] = lo;
        }

        void MTHI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            hi = gpr[rs];
        }

        void MTLO(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            lo = gpr[rs];
        }

        void Mult(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            long rsVal = gpr[rs];
            long rtVal = gpr[rt];
            long val = rsVal * rtVal;
            lo = (int)val;
            hi = (int)(val >> 32);
        }

        void MultU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            ulong rsVal = (uint)gpr[rs];
            ulong rtVal = (uint)gpr[rt];
            ulong val = rsVal * rtVal;
            lo = (int)val;
            hi = (int)(val >> 32);
        }

        void NOr(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = ~(gpr[rs] | gpr[rt]);
        }

        void Or(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] | gpr[rt];
        }

        void OrI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int imm = (ushort)inst.imm.Value;
            gpr[rt] = gpr[rs] | imm;
        }

        void SB(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            memory.Write(vAddr, (byte)gpr[rt], dataPos);
        }

        void SH(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            memory.Write(vAddr, (ushort)gpr[rt], shortPos);
        }

        void SLLV(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rt] << gpr[rs];
        }

        void SLT(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] < gpr[rt] ? 1 : 0;
        }

        void SLTI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int imm = inst.imm.Value;
            gpr[rt] = gpr[rs] < imm ? 1 : 0;
        }

        void SLTIU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            uint imm = (ushort)inst.imm.Value;
            gpr[rt] = (uint)gpr[rs] < imm ? 1 : 0;
        }

        void SLTU(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = (uint)gpr[rs] < (uint)gpr[rt] ? 1 : 0;
        }

        void SRA(Instruction inst)
        {
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            int sa = inst.shift.Value;
            gpr[rd] = gpr[rt] >> sa;
        }

        void SRAV(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rt] >> gpr[rs];
        }

        void SRL(Instruction inst)
        {
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            int sa = inst.shift.Value;
            gpr[rd] = (int)(((uint)gpr[rt]) >> sa);
        }

        void SRLV(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = (int)(((uint)gpr[rt]) >> gpr[rs]);
        }

        void Sub(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] - gpr[rt];
        }

        void SW(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int off = inst.off.Value;
            int vAddr = off + gpr[rs];
            memory.Write(vAddr, (uint) gpr[rt]);
        }

        void Xor(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int rd = (int)inst.rd.Value;
            gpr[rd] = gpr[rs] ^ gpr[rt];
        }

        void XorI(Instruction inst)
        {
            int rs = (int)inst.rs.Value;
            int rt = (int)inst.rt.Value;
            int imm = (ushort)inst.imm.Value;
            gpr[rt] = gpr[rs] ^ imm;
        }

        public Interpreter(uint[] ram)
        {
            memory = new Memory(ram);
            cmdToFunc = new Dictionary<Cmd, Performer>()
            {
                { Cmd.ADD,   Add },
                { Cmd.ADDU,  Add },
                { Cmd.ADDI,  AddI },
                { Cmd.ADDIU, AddI },
                { Cmd.AND,   And },
                { Cmd.ANDI,  AndI },
                { Cmd.DIV,   Div },
                { Cmd.DIVU,  DivU },
                { Cmd.LB,    LB },
                { Cmd.LBU,   LBU },
                { Cmd.LH,    LH },
                { Cmd.LHU,   LHU },
                { Cmd.LUI,   LUI },
                { Cmd.LW,    LW },
                { Cmd.LWU,   LW },
                { Cmd.MFHI,  MFHI },
                { Cmd.MFLO,  MFLO },
                { Cmd.MTHI,  MTHI },
                { Cmd.MTLO,  MTLO },
                { Cmd.MULT,  Mult },
                { Cmd.MULTU, MultU },
                { Cmd.NOR,   NOr },
                { Cmd.OR,    Or },
                { Cmd.ORI,   OrI },
                { Cmd.SB,    SB },
                { Cmd.SH,    SH },
                { Cmd.SLLV,  SLLV },
                { Cmd.SLT,   SLT },
                { Cmd.SLTI,  SLTI },
                { Cmd.SLTIU, SLTIU },
                { Cmd.SLTU,  SLTU },
                { Cmd.SRA,   SRA },
                { Cmd.SRAV,  SRAV },
                { Cmd.SRL,   SRL },
                { Cmd.SRLV,  SRLV },
                { Cmd.SUB,   Sub },
                { Cmd.SUBU,  Sub },
                { Cmd.SW,    SW },
                { Cmd.XOR,   Xor },
                { Cmd.XORI,  XorI },
            };
        }

        public void Execute(Instruction inst)
        {
            try
            {
                if (cmdToFunc.TryGetValue(inst.cmd, out Performer perform))
                    perform(inst);
            }
            catch (Exception) { }
        }

        public Instruction? GetInstruction()
        {
            uint cmd = memory.Read((int) pc);
            Instruction? inst = null;
            try
            {
                inst = decompiler.Decompile(cmd);
            }
            catch(Exception) { }
            pc += 4;
            return inst;
        }
    }
}
