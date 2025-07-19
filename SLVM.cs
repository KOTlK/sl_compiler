using System;
using System.Text;
using System.Collections.Generic;

using static Opcode;
using static Assertions;
using static BytecodeConstants;
/*
    Stack Frame:

 any size      4B       4B         4B          1B           1B         2B each        2B each     any
args [n-0] | old pc | prev fp | ret size | localsCount | argsCount | arg offsets | local offsets | locals | stack |
                                                                   ^
                                                                   |
                                                       New frame pointer points here
*/
public static unsafe class SLVM {
    public static byte[]            Stack;
    public static uint              StackCurrent;

    private static ErrorStream Err;

    private const int  StackSize = 1024 * 1024 * 8; // 8mb stack
    private const uint FrameHeader = 14;
    private const uint ArgsCountOffset      = 1;
    private const uint LocalsCountOffset    = 2;
    private const uint RetSizeOffset        = 6;
    private const uint PrevFpOffset         = 10;
    private const uint OldPcOffset          = 14;

    public static void Init(ErrorStream err) {
        Stack  = null;
        Stack  = new byte[StackSize];
        Err    = err;
    }

    public static int Run(CodeUnit exe, ErrorStream err) {
        StackCurrent = 0;
        Err        = err;
        var  bytes = exe.Bytes;
        var  count = exe.Count;
        uint pc    = 0;
        Readu32(bytes, ref pc);

        if (bytes[0] != 0x80 ||
            bytes[1] != 0x00 ||
            bytes[2] != 0x00 ||
            bytes[3] != 0x8A) {

            err.Push("Incorrent executable. Wrong mask");
            return 1;
        }

        uint main = GetFunctionByIndex(bytes, 0);
		pc = main;
        var  mainArgCount = Readu8(bytes, ref pc);
        var  mainLocCount = Readu8(bytes, ref pc);
        var  mainRetSize  = Readu32(bytes, ref pc);

        var fp   = pc; // frame pointer

        StackPush(pc);
        StackPush(fp);
        StackPush(mainRetSize);
        StackPush(mainLocCount);
        StackPush(mainArgCount);
        fp = StackCurrent;
        var mainFp = fp;

        for (var i = 0; i < mainArgCount; ++i) {
            StackPush(Readu16(bytes, ref pc));
        }

        ushort locOffset = 0;
        for (var i = 0; i < mainLocCount; ++i) {
            locOffset = Readu16(bytes, ref pc);
            StackPush(locOffset);
        }
        // reserve space for local variables
        StackPushZeros(locOffset);

        while (pc < count) {
            var opcode = ReadCode(bytes, ref pc);

            switch (opcode) {
                case func : {
                } break;
                case call : {
                    var index     = Readu32(bytes, ref pc);
                    var newPc     = GetFunctionByIndex(bytes, index);
                    var oldPc     = pc;
                    pc = newPc;
                    var argsCount   = Readu8(bytes, ref pc);
                    var localsCount = Readu8(bytes, ref pc);
                    var retSize     = Readu32(bytes, ref pc);
                    StackPush(oldPc);
                    StackPush(fp);
                    StackPush(retSize);
                    StackPush(localsCount);
                    StackPush(argsCount);
                    fp = StackCurrent;

                    for (var i = 0; i < argsCount; ++i) {
                        StackPush(Readu16(bytes, ref pc));
                    }

                    ushort localOffset = 0;
                    for (var i = 0; i < localsCount; ++i) {
                        localOffset = Readu16(bytes, ref pc);
                        StackPush(localOffset);
                    }
                    // reserve space for local variables
                    StackPushZeros(localOffset);
                } break;
                case add_s32 : {
                    var a = StackPops32();
                    var b = StackPops32();
                    StackPush(a + b);
                } break;
                case sub_s32 : {
                    var a = StackPops32();
                    var b = StackPops32();
                    StackPush(a - b);
                } break;
                case ret : {
                    if (fp == mainFp) {
                        var ret = StackPops32();
                        // я ебал майкрософт.
                        var aCount   = ReadArgsCount(fp);
                        uint bck       = FrameHeader;

                        if (aCount != 0) {
                            bck += ReadArgOffset((byte)(aCount - 1), fp);
                        }

                        StackCurrent = fp - bck;
                        return ret;
                    }

                    var oldPc       = ReadOldPc(fp);
                    var oldFp       = ReadOldFp(fp);
                    var retSize     = ReadRetSize(fp);
                    var localsCount = ReadLocalsCount(fp);
                    var argsCount   = ReadArgsCount(fp);
                    uint back       = FrameHeader;

                    if (argsCount != 0) {
                        back += ReadArgOffset((byte)(argsCount - 1), fp);
                    }

                    StackCurrent = fp - back;
                    ushort localsOffset = 0;

                    if (localsCount > 0) {
                        localsOffset = ReadLocalOffset((byte)(localsCount - 1), fp);
                    }
                    var start = fp + (uint)argsCount * 2 + (uint)localsCount * 2 + localsOffset;
                    StackPush(start, retSize);
                    fp = oldFp;
                    pc = oldPc;
                } break;
                case push_s32 : {
                    StackPush(Reads32(bytes, ref pc));
                } break;
                case push_size : {
                    var cnt = Readu32(bytes, ref pc);
                    StackPushZeros(cnt);
                } break;
                case pop_s32 : {
                    StackPops32();
                    break;
                }
                case larg : {
                    var index = Readu8(bytes, ref pc);
                    StackPushArg(index, fp);
                    break;
                }
                case llocal : {
                    var index = Readu8(bytes, ref pc);
                    StackPushLocal(index, fp);
                } break;
                case slocal : {
                    var index = Readu8(bytes, ref pc);
                    StackSetLocal(index, fp);
                } break;
                default : {
                    err.Push("Unknown opcode at %", pc);
                    return 3;
                }
            }
        }

        return StackPops32();
    }

    public static byte ReadArgsCount(uint fp) {
        return Stack[fp - ArgsCountOffset];
    }

    public static byte ReadLocalsCount(uint fp) {
        return Stack[fp - LocalsCountOffset];
    }

    public static uint ReadRetSize(uint fp) {
        var s = (uint)(Stack[fp - RetSizeOffset]           |
                       Stack[fp - RetSizeOffset - 1] << 8  |
                       Stack[fp - RetSizeOffset - 2] << 16 |
                       Stack[fp - RetSizeOffset - 3] << 24);

        return s;
    }

    public static uint ReadOldFp(uint fp) {
        var s = (uint)(Stack[fp - PrevFpOffset]           |
                       Stack[fp - PrevFpOffset - 1] << 8  |
                       Stack[fp - PrevFpOffset - 2] << 16 |
                       Stack[fp - PrevFpOffset - 3] << 24);

        return s;
    }

    public static uint ReadOldPc(uint fp) {
        var s = (uint)(Stack[fp - OldPcOffset]           |
                       Stack[fp - OldPcOffset - 1] << 8  |
                       Stack[fp - OldPcOffset - 2] << 16 |
                       Stack[fp - OldPcOffset - 3] << 24);

        return s;
    }

    public static Opcode ReadCode(byte[] bytes, ref uint ptr) {
        var opcode = (Opcode)Readu16(bytes, ref ptr);
        return opcode;
    }

    public static long Reads64(byte[] bytes, ref uint ptr) {
        var i = (long)(bytes[ptr]           |
                       bytes[ptr + 1] << 8  |
                       bytes[ptr + 2] << 16 |
                       bytes[ptr + 3] << 24 |
                       bytes[ptr + 4] << 32 |
                       bytes[ptr + 5] << 40 |
                       bytes[ptr + 6] << 48 |
                       bytes[ptr + 7] << 56);

        ptr += 8;

        return i;
    }

    public static ulong Readu64(byte[] bytes, ref uint ptr) {
        var i = (ulong)(bytes[ptr]           |
                        bytes[ptr + 1] << 8  |
                        bytes[ptr + 2] << 16 |
                        bytes[ptr + 3] << 24 |
                        bytes[ptr + 4] << 32 |
                        bytes[ptr + 5] << 40 |
                        bytes[ptr + 6] << 48 |
                        bytes[ptr + 7] << 56);

        ptr += 8;

        return i;
    }

    public static int Reads32(byte[] bytes, ref uint ptr) {
        var i = bytes[ptr]           |
                bytes[ptr + 1] << 8  |
                bytes[ptr + 2] << 16 |
                bytes[ptr + 3] << 24;

        ptr += 4;

        return i;
    }

    public static uint Readu32(byte[] bytes, ref uint ptr) {
        var i = (uint)(bytes[ptr]           |
                       bytes[ptr + 1] << 8  |
                       bytes[ptr + 2] << 16 |
                       bytes[ptr + 3] << 24);

        ptr += 4;

        return i;
    }

    public static short Reads16(byte[] bytes, ref uint ptr) {
        var i = (short)(bytes[ptr] |
                        bytes[ptr + 1] << 8);

        ptr += 2;

        return i;
    }

    public static ushort Readu16(byte[] bytes, ref uint ptr) {
        var i = (ushort)(bytes[ptr] |
                         bytes[ptr + 1] << 8);

        ptr += 2;

        return i;
    }

    public static sbyte Reads8(byte[] bytes, ref uint ptr) {
        var i = (sbyte)(bytes[ptr++]);

        return i;
    }

    public static byte Readu8(byte[] bytes, ref uint ptr) {
        var i = bytes[ptr++];

        return i;
    }

    public static float Readfloat(byte[] bytes, ref uint ptr) {
        var i = Readu32(bytes, ref ptr);
        return *(float*)&i;
    }

    public static double Readdouble(byte[] bytes, ref uint ptr) {
        var i = Readu64(bytes, ref ptr);
        return *(double*)&i;
    }

    public static void StackPush(float f) {
        if (StackCurrent + 4 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        uint *intPtr = (uint*)&f;
        uint intVal = *intPtr;

        Stack[StackCurrent]     = (byte)( intVal        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((intVal >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((intVal >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((intVal >> 24) & 0xFF);

        StackCurrent += 4;
    }

    public static void StackPush(double d) {
        if (StackCurrent + 8 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        ulong *intPtr = (ulong*)&d;
        ulong intVal = *intPtr;

        Stack[StackCurrent]     = (byte)( intVal        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((intVal >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((intVal >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((intVal >> 24) & 0xFF);
        Stack[StackCurrent + 4] = (byte)((intVal >> 32) & 0xFF);
        Stack[StackCurrent + 5] = (byte)((intVal >> 40) & 0xFF);
        Stack[StackCurrent + 6] = (byte)((intVal >> 48) & 0xFF);
        Stack[StackCurrent + 7] = (byte)((intVal >> 56) & 0xFF);

        StackCurrent += 8;
    }

    public static void StackPush(int a) {
        if (StackCurrent + 4 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((a >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((a >> 24) & 0xFF);

        StackCurrent += 4;
    }

    public static void StackPush(uint a) {
        if (StackCurrent + 4 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((a >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((a >> 24) & 0xFF);

        StackCurrent += 4;
    }

    public static void StackPush(short a) {
        if (StackCurrent + 2 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);

        StackCurrent += 2;
    }

    public static void StackPush(ushort a) {
        if (StackCurrent + 2 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);

        StackCurrent += 2;
    }

    public static void StackPush(long a) {
        if (StackCurrent + 8 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((a >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((a >> 24) & 0xFF);
        Stack[StackCurrent + 4] = (byte)((a >> 32) & 0xFF);
        Stack[StackCurrent + 5] = (byte)((a >> 40) & 0xFF);
        Stack[StackCurrent + 6] = (byte)((a >> 48) & 0xFF);
        Stack[StackCurrent + 7] = (byte)((a >> 56) & 0xFF);

        StackCurrent += 8;
    }

    public static void StackPush(ulong a) {
        if (StackCurrent + 8 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent]     = (byte)( a        & 0xFF);
        Stack[StackCurrent + 1] = (byte)((a >> 8)  & 0xFF);
        Stack[StackCurrent + 2] = (byte)((a >> 16) & 0xFF);
        Stack[StackCurrent + 3] = (byte)((a >> 24) & 0xFF);
        Stack[StackCurrent + 4] = (byte)((a >> 32) & 0xFF);
        Stack[StackCurrent + 5] = (byte)((a >> 40) & 0xFF);
        Stack[StackCurrent + 6] = (byte)((a >> 48) & 0xFF);
        Stack[StackCurrent + 7] = (byte)((a >> 56) & 0xFF);

        StackCurrent += 8;
    }

    public static void StackPush(byte b) {
        if (StackCurrent + 1 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent++] = b;
    }

    public static void StackPush(sbyte b) {
        if (StackCurrent + 1 >= StackSize) {
            Err.Push("Stack overflow");
            return;
        }

        Stack[StackCurrent++] = (byte)b;
    }

    public static long StackPops64() {
        var i = (long)(Stack[StackCurrent - 8]       |
                       Stack[StackCurrent - 7] << 8  |
                       Stack[StackCurrent - 6] << 16 |
                       Stack[StackCurrent - 5] << 24 |
                       Stack[StackCurrent - 4] << 32 |
                       Stack[StackCurrent - 3] << 40 |
                       Stack[StackCurrent - 2] << 48 |
                       Stack[StackCurrent - 1] << 56);

        StackCurrent -= 8;

        return i;
    }

    public static ulong StackPopu64() {
        var i = (ulong)(Stack[StackCurrent - 8]       |
                        Stack[StackCurrent - 7] << 8  |
                        Stack[StackCurrent - 6] << 16 |
                        Stack[StackCurrent - 5] << 24 |
                        Stack[StackCurrent - 4] << 32 |
                        Stack[StackCurrent - 3] << 40 |
                        Stack[StackCurrent - 2] << 48 |
                        Stack[StackCurrent - 1] << 56);

        StackCurrent -= 8;

        return i;
    }

    public static int StackPops32() {
        var i = Stack[StackCurrent - 4]       |
                Stack[StackCurrent - 3] << 8  |
                Stack[StackCurrent - 2] << 16 |
                Stack[StackCurrent - 1] << 24;

        StackCurrent -= 4;

        return i;
    }

    public static uint StackPopu32() {
        var i = (uint)(Stack[StackCurrent - 4]       |
                       Stack[StackCurrent - 3] << 8  |
                       Stack[StackCurrent - 2] << 16 |
                       Stack[StackCurrent - 1] << 24);

        StackCurrent -= 4;

        return i;
    }

    public static short StackPops16() {
        var i = (short)(Stack[StackCurrent - 2] |
                        Stack[StackCurrent - 1] << 8);

        StackCurrent -= 2;

        return i;
    }

    public static ushort StackPopu16() {
        var i = (ushort)(Stack[StackCurrent - 2] |
                         Stack[StackCurrent - 1] << 8);

        StackCurrent -= 2;

        return i;
    }

    public static sbyte StackPops8() {
        var i = (sbyte)(Stack[--StackCurrent]);

        return i;
    }

    public static byte StackPopu8() {
        var i = Stack[--StackCurrent];

        return i;
    }

    public static float StackPopfloat() {
        var i = StackPopu32();
        return *(float*)&i;
    }

    public static double StackPopdouble() {
        var i = StackPopu64();
        return *(double*)&i;
    }

    public static void StackPushArg(byte index, uint fp) {
        var offset  = ReadArgOffset(index, fp);
        var size    = offset;

        // if index == 0 size = offset
        if(index != 0) {
            size -= ReadArgOffset((byte)(index - 1), fp);
        }

        var start   = (fp - FrameHeader) - offset;

        StackPush(start, size);
    }

    public static ushort ReadArgOffset(byte index, uint fp) {
        var start = fp + index * 2;

        var s = (ushort)(Stack[start + 0]   |
                         Stack[start + 1] << 8);

        return s;
    }

    // @Incomplete
    public static void StackSetLocal(byte index, uint fp) {
        var argsCount   = (uint)ReadArgsCount(fp);
        var localsCount = (uint)ReadLocalsCount(fp);
        var offset      = ReadLocalOffset(index, fp);
        var size        = offset;

        if (index == 0) {
            offset = 0;
        } else {
            // why the fuck byte - 1 is int expression???????????????
            size   -= ReadLocalOffset((byte)(index - 1), fp);
            offset -= size;
        }

        var start = fp + argsCount * 2 + localsCount * 2 + offset;

        StackSet(StackCurrent - size, start, size);
        StackCurrent -= size;
    }

    // @Debug
    public static int StackReadLocal(byte index, uint fp) {
        var argsCount   = (uint)ReadArgsCount(fp);
        var localsCount = (uint)ReadLocalsCount(fp);
        var offset      = ReadLocalOffset(index, fp);
        var size        = offset;

        if (index == 0) {
            offset = 0;
        } else {
            size   -= ReadLocalOffset((byte)(index - 1), fp);
            offset -= size;
        }

        var start = (fp + argsCount * 2 + localsCount * 2) + offset;

        var i = Stack[start]           |
                Stack[start + 1] << 8  |
                Stack[start + 2] << 16 |
                Stack[start + 3] << 24;

        return i;
    }

    public static void StackPushLocal(byte index, uint fp) {
        var argsCount   = (uint)ReadArgsCount(fp);
        var localsCount = (uint)ReadLocalsCount(fp);
        var offset      = ReadLocalOffset(index, fp);
        var size        = offset;

        if (index == 0) {
            offset = 0;
        } else {
            size   -= ReadLocalOffset((byte)(index - 1), fp);
            offset -= size;
        }

        var start = (fp + argsCount * 2 + localsCount * 2) + offset;

        StackPush(start, size);
    }

    public static ushort ReadLocalOffset(byte index, uint fp) {
        var argsCount = (uint)ReadArgsCount(fp);
        var start     = (fp + argsCount * 2) + index * 2;

        var s = (ushort)(Stack[start + 0]   |
                         Stack[start + 1] << 8);

        return s;
    }

    public static void StackSet(uint from, uint to, uint size) {
        for (uint i = 0; i < size; ++i) {
            Stack[to + i] = Stack[from + i];
        }
    }

    public static void StackPush(uint from, uint size) {
        for (uint i = 0; i < size; ++i) {
            Stack[StackCurrent + i] = Stack[from + i];
        }

        StackCurrent += size;
    }

    public static void StackPushZeros(uint count) {
        for (uint i = 0; i < count; ++i) {
            Stack[StackCurrent + i] = 0;
        }
        StackCurrent += count;
    }

    private static uint GetFunctionByIndex(byte[] bytes, uint index) {
        var funCount = Readu32(bytes, FunctionsCountOffset);
        if (index >= funCount) {
            Err.Push("Function index % is out of range %-%", index, 0, funCount);
            return 0;
        }

        // 8 is size of funcIndex(4B) + funPos(4B)
        var offset = 8 * index;

        Assert(Readu32(bytes, FunctionsOffset + offset) == index, $"Function indices are not identical! Trying to get {index}, got {Readu32(bytes, FunctionsOffset + offset)}");

        offset += 4;

        return Readu32(bytes, FunctionsOffset + offset);
    }

    private static long Reads64(byte[] bytes, uint ptr) {
        var i = (long)(bytes[ptr]           |
                       bytes[ptr + 1] << 8  |
                       bytes[ptr + 2] << 16 |
                       bytes[ptr + 3] << 24 |
                       bytes[ptr + 4] << 32 |
                       bytes[ptr + 5] << 40 |
                       bytes[ptr + 6] << 48 |
                       bytes[ptr + 7] << 56);

        return i;
    }

    private static ulong Readu64(byte[] bytes, uint ptr) {
        var i = (ulong)(bytes[ptr]           |
                        bytes[ptr + 1] << 8  |
                        bytes[ptr + 2] << 16 |
                        bytes[ptr + 3] << 24 |
                        bytes[ptr + 4] << 32 |
                        bytes[ptr + 5] << 40 |
                        bytes[ptr + 6] << 48 |
                        bytes[ptr + 7] << 56);

        return i;
    }

    private static int Reads32(byte[] bytes, uint ptr) {
        var i = bytes[ptr]           |
                bytes[ptr + 1] << 8  |
                bytes[ptr + 2] << 16 |
                bytes[ptr + 3] << 24;

        return i;
    }

    private static uint Readu32(byte[] bytes, uint ptr) {
        var i = (uint)(bytes[ptr]           |
                       bytes[ptr + 1] << 8  |
                       bytes[ptr + 2] << 16 |
                       bytes[ptr + 3] << 24);

        return i;
    }

    private static short Reads16(byte[] bytes, uint ptr) {
        var i = (short)(bytes[ptr] |
                        bytes[ptr + 1] << 8);

        return i;
    }

    private static ushort Readu16(byte[] bytes, uint ptr) {
        var i = (ushort)(bytes[ptr] |
                         bytes[ptr + 1] << 8);

        return i;
    }

    private static sbyte Reads8(byte[] bytes, uint ptr) {
        var i = (sbyte)(bytes[ptr]);

        return i;
    }

    private static byte Readu8(byte[] bytes, uint ptr) {
        var i = bytes[ptr];

        return i;
    }

    private static float Readfloat(byte[] bytes, uint ptr) {
        var i = Readu32(bytes, ptr);
        return *(float*)&i;
    }

    private static double Readdouble(byte[] bytes, uint ptr) {
        var i = Readu64(bytes, ptr);
        return *(double*)&i;
    }

    public static string BytecodeToString(byte[] bytes, uint count) {
        var  sb = new StringBuilder();
        uint pc = 4;

        // print function table
        var funCount = Readu32(bytes, ref pc);

        for (var i = 0; i < funCount; ++i) {
            var index = Readu32(bytes, ref pc);
            var pos   = Readu32(bytes, ref pc);

            sb.Append(index);
            sb.Append(':');
            sb.Append(pos);
            sb.Append('\n');
        }

        while (pc < count) {
            var opcode = ReadCode(bytes, ref pc);

            switch (opcode) {
                case func : {
                    sb.Append(".func");
                    var argCount = Readu8(bytes, ref pc);
                    sb.Append(' ');
                    sb.Append(argCount.ToString());
                    var localsCount = Readu8(bytes, ref pc);
                    sb.Append(' ');
                    sb.Append(localsCount.ToString());
                    var retSize = Readu32(bytes, ref pc);
                    sb.Append(' ');
                    sb.Append(retSize.ToString());

                    for (int i = 0; i < argCount; ++i) {
                        sb.Append(' ');
                        sb.Append(Readu16(bytes, ref pc));
                    }

                    for (int i = 0; i < localsCount; ++i) {
                        sb.Append(' ');
                        sb.Append(Readu16(bytes, ref pc));
                    }
                } break;
                case call : {
                    sb.Append("call ");
                    var index = Reads32(bytes, ref pc);
                    sb.Append(index.ToString());
                } break;
                case add_s32 : {
                    sb.Append(opcode.ToString());
                } break;
                case sub_s32 : {
                    sb.Append(opcode.ToString());
                } break;
                case ret : {
                    sb.Append(opcode.ToString());
                } break;
                case push_s32 : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var val = Reads32(bytes, ref pc);
                    sb.Append(val.ToString());
                } break;
                case push_size : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var val = Reads32(bytes, ref pc);
                    sb.Append(val.ToString());
                } break;
                case larg : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var index = Readu8(bytes, ref pc);
                    sb.Append(index.ToString());
                } break;
                case llocal : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var index = Readu8(bytes, ref pc);
                    sb.Append(index.ToString());
                } break;
                case slocal : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var index = Readu8(bytes, ref pc);
                    sb.Append(index.ToString());
                } break;
                default : {
                    sb.Append("Unknown operation");
                    break;
                }
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }
}