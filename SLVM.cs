using System;
using System.Text;
using System.Collections.Generic;

using static Opcode;
using static Assertions;
using static BytecodeConstants;
using static Context;
using static Register;

public struct StackFrame {
    public int  Index;
    public uint RegStart;
    public uint RegEnd;
    public uint OldPc;
    public uint Fp;
}

public static unsafe class SLVM {
    public static Register[]        Registers;
    public static byte[]            Stack;
    public static StackFrame[]      Frames;
    public static uint              StackCurrent;
    public static int               CurrentFrame;

    private const int MaxFrames = 4096;

    public static void Init() {
        Stack        = null;
        Stack        = new byte[StackSize];
        Registers    = new Register[RegistersCount];
        Frames       = new StackFrame[MaxFrames];
        CurrentFrame = -1;
    }

    public static int Run(CodeUnit exe) {
        StackCurrent = 0;
		CurrentFrame = -1;
        var  bytes = exe.Bytes;
        var  count = exe.Count;
        uint pc    = 0;
        Readu32(bytes, ref pc);

        if (bytes[0] != 0x80 ||
            bytes[1] != 0x00 ||
            bytes[2] != 0x00 ||
            bytes[3] != 0x8A) {

            Err.Push("Incorrent executable. Wrong mask");
            return 1;
        }

        uint main = GetFunctionByIndex(bytes, 0);
		pc = main;
		var mainRegCount  = Readu16(bytes, ref pc);
		Readu16(bytes, ref pc); // dont need args count

        var sf = PushStackFrame(StackCurrent, pc, 0, (uint)mainRegCount - 1);

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
                    var regCount  = Readu16(bytes, ref pc);
                    var argCount  = Readu16(bytes, ref pc);
                    var regStart  = sf.RegEnd - (argCount - (uint)1); // is this a joke?
                    var regEnd    = regStart + regCount - 1;

                    sf = PushStackFrame(StackCurrent, oldPc, regStart, regEnd);
                } break;
                case add : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.s32 = a.s32 + b.s32;
                            break;
                        }
                        case 1 : {
                            dest.u32 = a.u32 + b.u32;
                            break;
                        }
                        case 2 : {
                            dest.s64 = a.s64 + b.s64;
                            break;
                        }
                        case 3 : {
                            dest.u64 = a.u64 + b.u64;
                            break;
                        }
                    }
                } break;
                case sub : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.s32 = a.s32 - b.s32;
                            break;
                        }
                        case 1 : {
                            dest.u32 = a.u32 - b.u32;
                            break;
                        }
                        case 2 : {
                            dest.s64 = a.s64 - b.s64;
                            break;
                        }
                        case 3 : {
                            dest.u64 = a.u64 - b.u64;
                            break;
                        }
                    }
                } break;
                case mul : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.s32 = a.s32 * b.s32;
                            break;
                        }
                        case 1 : {
                            dest.u32 = a.u32 * b.u32;
                            break;
                        }
                        case 2 : {
                            dest.s64 = a.s64 * b.s64;
                            break;
                        }
                        case 3 : {
                            dest.u64 = a.u64 * b.u64;
                            break;
                        }
                    }
                } break;
                case div : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.s32 = a.s32 / b.s32;
                            break;
                        }
                        case 1 : {
                            dest.u32 = a.u32 / b.u32;
                            break;
                        }
                        case 2 : {
                            dest.s64 = a.s64 / b.s64;
                            break;
                        }
                        case 3 : {
                            dest.u64 = a.u64 / b.u64;
                            break;
                        }
                    }
                } break;
                case mod : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.s32 = a.s32 % b.s32;
                            break;
                        }
                        case 1 : {
                            dest.u32 = a.u32 % b.u32;
                            break;
                        }
                        case 2 : {
                            dest.s64 = a.s64 % b.s64;
                            break;
                        }
                        case 3 : {
                            dest.u64 = a.u64 % b.u64;
                            break;
                        }
                    }
                } break;
                case fadd : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.Float = a.Float + b.Float;
                            break;
                        }
                        case 1 : {
                            dest.Double = a.Double + b.Double;
                            break;
                        }
                    }
                } break;
                case fsub : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.Float = a.Float - b.Float;
                            break;
                        }
                        case 1 : {
                            dest.Double = a.Double - b.Double;
                            break;
                        }
                    }
                } break;
                case fmul : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.Float = a.Float * b.Float;
                            break;
                        }
                        case 1 : {
                            dest.Double = a.Double * b.Double;
                            break;
                        }
                    }
                } break;
                case fdiv : {
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.Float = a.Float / b.Float;
                            break;
                        }
                        case 1 : {
                            dest.Double = a.Double / b.Double;
                            break;
                        }
                    }
                } break;
                case fmod : { // (C)
                    // Garbage in - garbage out
                    var regType  = Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case 0 : {
                            dest.Float = a.Float % b.Float;
                            break;
                        }
                        case 1 : {
                            dest.Double = a.Double % b.Double;
                            break;
                        }
                    }
                } break;
                case ret : {
                    var regId   = Readu16(bytes, ref pc);
                    ref var reg = ref GetRegister(regId);

                    if (CurrentFrame == 0) {
                        return reg.s32;
                    }

                    pc = sf.OldPc;

                    Registers[sf.RegStart] = reg;

                    sf = PopStackFrame();
                } break;
                case mov : {
                    var reg = Readu16(bytes, ref pc);
                    ref var src  = ref GetRegister(Readu16(bytes, ref pc));

                    Registers[reg] = src;
                } break;
                case set_s32 : {
                    ref var reg = ref GetRegister(Readu16(bytes, ref pc));
                    reg.Type = RegisterType.s32;
                    reg.s32  = Reads32(bytes, ref pc);
                } break;
                default : {
                    Err.Push("Unknown opcode at %", pc);
                    return 3;
                }
            }
        }

        return 3;
    }

    public static StackFrame PushStackFrame(uint fp,
                                            uint oldpc,
                                            uint regStart,
                                            uint regEnd) {
        var sf = new StackFrame();
        CurrentFrame += 1;

        if (CurrentFrame >= MaxFrames) {
            Err.Push("Stack Overflow caused by huge amount of stack frames.");
            return Frames[0];
        }

        sf.Index    = CurrentFrame;
        sf.Fp       = fp;
        sf.OldPc    = oldpc;
        sf.RegStart = regStart;
        sf.RegEnd   = regEnd;

        Frames[CurrentFrame] = sf;

        if (regEnd >= RegistersCount) {
            Err.Push("Registers overflow");
            return Frames[0];
        }

        return sf;
    }

    public static StackFrame PopStackFrame() {
        var sf = Frames[CurrentFrame];
        CurrentFrame -= 1;

        StackCurrent = sf.Fp;
        return Frames[CurrentFrame];
    }

    public static ref StackFrame GetCurrentStackFrame() {
        return ref Frames[CurrentFrame];
    }

    public static ref Register GetRegister(uint index) {
        ref var sf = ref GetCurrentStackFrame();
        return ref Registers[sf.RegStart + index];
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

    // public static void StackPush(Register v) {
    //     Registers[StackCurrent++] = v;
    // }

    // public static Register StackPop() {
    //     return Registers[--StackCurrent];
    // }

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
                    var regCount = Readu16(bytes, ref pc);
                    sb.Append(' ');
                    sb.Append(regCount.ToString());
                    var argCount = Readu16(bytes, ref pc);
                    sb.Append(' ');
                    sb.Append(argCount.ToString());
                } break;
                case call : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var index = Readu32(bytes, ref pc);
                    sb.Append(index.ToString());
                    // sb.Append(' ');
                    // var argReg = Readu16(bytes, ref pc);
                    // sb.Append(argReg.ToString());
                } break;
                case mov : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    sb.Append(Readu16(bytes, ref pc).ToString());
                    sb.Append(' ');
                    sb.Append(Readu16(bytes, ref pc).ToString());
                    sb.Append(' ');
                } break;
                case add  :
                case fadd :
                case sub  :
                case fsub :
                case mul  :
                case fmul :
                case div  :
                case fdiv :
                case mod  :
                case fmod : { // (C)
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var regType = Readu8(bytes, ref pc);
                    var dest = Readu16(bytes, ref pc);
                    var r0   = Readu16(bytes, ref pc);
                    var r1   = Readu16(bytes, ref pc);
                    sb.Append(regType.ToString());
                    sb.Append(' ');
                    sb.Append(dest.ToString());
                    sb.Append(' ');
                    sb.Append(r0.ToString());
                    sb.Append(' ');
                    sb.Append(r1.ToString());
                } break;
                case set_s32 : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var reg      = Readu16(bytes, ref pc);
                    var constant = Reads32(bytes, ref pc);
                    sb.Append(reg.ToString());
                    sb.Append(' ');
                    sb.Append(constant.ToString());
                } break;
                case ret : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var reg = Readu16(bytes, ref pc);
                    sb.Append(reg.ToString());
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