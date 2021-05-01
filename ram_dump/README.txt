This documents describes how Star Display was ported (at least the base functionality) to decomp.

The main address that Star Display is using to show the stars is 80207700 which corresponds to EEPROM copy in the RAM. In decomp this address can be relocated anywhere, it corresponds to 'gSaveBuffer' symbol. From Star Display design standpoint I did not want to hardcode any addresses but use heuristic techniques to find the value for 'gSaveBuffer' and other interesting values. 

Commonly used method is dynamic emulation via interpreter of RAM regions. A very trivial emulator is written in MIPSInterpreter folder, it allows to access MIPS GPR for any arguments passed to a function + it emulates loads from RAM, load/stores to SP (unused currently though). MIPSInterpreter assumes that virtual addresses have first 16bit related to the segment, 0x80 is RAM and 0x81 is stack (which is not in RAM because I prohibit stores to RAM).

The heuristic used in Star Display to find 'gSaveBuffer' is a very lucky coincidence that 'bzero' function is implemented in libultra that is not recompiled (at least currently it is in a binary format) and taken as is. Hence compiled 'bzero' function is the same in decomp and legacy sm64 roms. This makes it possible to create 'JAL' instruction to 'bzero' for decomp ROMs. 'bzero' has the following prototype: "extern void bzero(void *, size_t);", so A0='gSaveBuffer' and A1='sizeof(gSaveBuffer)'

Another lucky coincidence that 'bzero' is function is barely ever used - there is around 10 usages in the whole core, 3 of them are related to cleaning 'gSaveBuffer' parts. This makes it possible to just iterate over the whole RAM and find all the 'JALs' to 'bzero' and emulate a few instructions (16 when this document was written) before the jump. Because 'bzero' is also called multiple times for the close parts 'gSaveBuffer', it is possible to validate that acquire address seems like 'gSaveBuffer' by checking if there is another 'bzero' in EEPROM_SIZE (0x200 or 0x400) bytes from detected buffer.

This makes the following algorithm:
1) Find all RAM address of 'bzero' function.
2) For each RAM address create a 'JAL' command
3) For each 'JAL' command find all 4byte addresses in RAM
4) For each 'JAL' addresses decompile previous ~16 instructions and monitor A0 and A1 registers
5) Check if A0 and A1 are reasonable - A0 must have 0x80 and be lower than 0x800000, A1 must be lower than 0x1000
6) Iterate over all pointers, find ones that calls 'bzero' with size 0x200 or 0x400
7) If there is a pointer in next 0x200 or 0x400 bytes that is also 'bzero'd, call pointer acquired in step6 'gSaveBuffer'

If you want to play around with RAM, there are some examples available. You may feed it in Program.cs in MIPSInterpreter and check how exactly DecompManager works.