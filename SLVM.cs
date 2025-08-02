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

public enum Flags : uint {
    Uninitialized = 0,
    Zero = 0x1,
    GT   = 0x2,
    LT   = 0x4,
    EQ   = 0x8,
}

public static unsafe class SLVM {
    public static Register[]        Registers;
    public static byte[]            Stack;
    public static StackFrame[]      Frames;
    public static uint              StackCurrent;
    public static int               CurrentFrame;

    public static uint[]            FunctionTable;
    public static uint[]            LabelTable;

    public static Flags FLAGS;

    private const int MaxFrames = 4096;

    public static void Init() {
        Stack        = null;
        Stack        = new byte[StackSize];
        Registers    = new Register[RegistersCount];
        Frames       = new StackFrame[MaxFrames];
        CurrentFrame = -1;
    }

    public static int Run(byte[] bytes) {
        StackCurrent = 0;
		CurrentFrame = -1;
        var  count = bytes.Length;
        uint pc    = 0;
        Readu32(bytes, ref pc);

        if (bytes[0] != 0x80 ||
            bytes[1] != 0x00 ||
            bytes[2] != 0x00 ||
            bytes[3] != 0x8A) {

            Err.Push("Incorrent executable. Wrong mask");
            return 1;
        }

        var funCount = Readu32(bytes, ref pc);
        FunctionTable = new uint[funCount];
        for (var i = 0; i < funCount; ++i) {
            var index = Readu32(bytes, ref pc);
            var pos   = Readu32(bytes, ref pc);

            FunctionTable[index] = pos;
        }

        var labelCount = Readu32(bytes, ref pc);
        LabelTable = new uint[labelCount];
        for (var i = 0; i < labelCount; ++i) {
            var index = Readu32(bytes, ref pc);
            var pos   = Readu32(bytes, ref pc);

            LabelTable[index] = pos;
        }

        uint main = GetFunctionByIndex(0);
		pc = main;
		var mainRegCount  = Readu16(bytes, ref pc);
		Readu16(bytes, ref pc); // dont need args count

        var sf = PushStackFrame(StackCurrent, pc, 0, (uint)mainRegCount - 1);

        while (pc < count) {
            var opcode = ReadCode(bytes, ref pc);

            switch (opcode) {
                case func  : break;
                case set : {
                    var regType = (RegType)Readu8(bytes, ref pc);
                    var dest    = Readu16(bytes, ref pc);
                    ref var reg = ref GetRegister(dest);

                    switch(regType) {
                        case RegType.s32 :
                            // reg.Type = RegisterType.s32;
                            reg.s32  = Reads32(bytes, ref pc);
                            break;
                        case RegType.u32:
                            // reg.Type = RegisterType.u32;
                            reg.u32  = Readu32(bytes, ref pc);
                            break;
                        case RegType.s64 :
                            // reg.Type = RegisterType.s64;
                            reg.s64  = Reads64(bytes, ref pc);
                            break;
                        case RegType.u64 :
                            // reg.Type = RegisterType.u64;
                            reg.u64  = Readu64(bytes, ref pc);
                            break;
                        case RegType.Float :
                            // reg.Type  = RegisterType.Float;
                            reg.Float = ReadFloat(bytes, ref pc);
                            break;
                        case RegType.Double :
                            // reg.Type   = RegisterType.Double;
                            reg.Double = ReadDouble(bytes, ref pc);
                            break;
                    }
                } break;
                case mov : {
                    var reg = Readu16(bytes, ref pc);
                    ref var src  = ref GetRegister(Readu16(bytes, ref pc));

                    Registers[reg] = src;
                } break;
                case add : {
                    // Garbage in - garbage out
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case RegType.s32 : {
                            dest.s32 = a.s32 + b.s32;
                            break;
                        }
                        case RegType.u32 : {
                            dest.u32 = a.u32 + b.u32;
                            break;
                        }
                        case RegType.s64 : {
                            dest.s64 = a.s64 + b.s64;
                            break;
                        }
                        case RegType.u64 : {
                            dest.u64 = a.u64 + b.u64;
                            break;
                        }
                        case RegType.Float : {
                            dest.Float = a.Float + b.Float;
                            break;
                        }
                        case RegType.Double : {
                            dest.Double = a.Double + b.Double;
                            break;
                        }
                    }
                } break;
                case sub : {
                    // Garbage in - garbage out
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case RegType.s32 : {
                            dest.s32 = a.s32 - b.s32;
                            break;
                        }
                        case RegType.u32 : {
                            dest.u32 = a.u32 - b.u32;
                            break;
                        }
                        case RegType.s64 : {
                            dest.s64 = a.s64 - b.s64;
                            break;
                        }
                        case RegType.u64 : {
                            dest.u64 = a.u64 - b.u64;
                            break;
                        }
                        case RegType.Float : {
                            dest.Float = a.Float - b.Float;
                            break;
                        }
                        case RegType.Double : {
                            dest.Double = a.Double - b.Double;
                            break;
                        }
                    }
                } break;
                case mul : {
                    // Garbage in - garbage out
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case RegType.s32 : {
                            dest.s32 = a.s32 * b.s32;
                            break;
                        }
                        case RegType.u32 : {
                            dest.u32 = a.u32 * b.u32;
                            break;
                        }
                        case RegType.s64 : {
                            dest.s64 = a.s64 * b.s64;
                            break;
                        }
                        case RegType.u64 : {
                            dest.u64 = a.u64 * b.u64;
                            break;
                        }
                        case RegType.Float : {
                            dest.Float = a.Float * b.Float;
                            break;
                        }
                        case RegType.Double : {
                            dest.Double = a.Double * b.Double;
                            break;
                        }
                    }
                } break;
                case div : {
                    // Garbage in - garbage out
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case RegType.s32 : {
                            dest.s32 = a.s32 / b.s32;
                            break;
                        }
                        case RegType.u32 : {
                            dest.u32 = a.u32 / b.u32;
                            break;
                        }
                        case RegType.s64 : {
                            dest.s64 = a.s64 / b.s64;
                            break;
                        }
                        case RegType.u64 : {
                            dest.u64 = a.u64 / b.u64;
                            break;
                        }
                        case RegType.Float : {
                            dest.Float = a.Float / b.Float;
                            break;
                        }
                        case RegType.Double : {
                            dest.Double = a.Double / b.Double;
                            break;
                        }
                    }
                } break;
                case mod : {
                    // Garbage in - garbage out
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    ref var dest = ref GetRegister(Readu16(bytes, ref pc));
                    ref var a    = ref GetRegister(Readu16(bytes, ref pc));
                    ref var b    = ref GetRegister(Readu16(bytes, ref pc));

                    switch(regType) {
                        case RegType.s32 : {
                            dest.s32 = a.s32 % b.s32;
                            break;
                        }
                        case RegType.u32 : {
                            dest.u32 = a.u32 % b.u32;
                            break;
                        }
                        case RegType.s64 : {
                            dest.s64 = a.s64 % b.s64;
                            break;
                        }
                        case RegType.u64 : {
                            dest.u64 = a.u64 % b.u64;
                            break;
                        }
                        case RegType.Float : {
                            dest.Float = a.Float % b.Float;
                            break;
                        }
                        case RegType.Double : {
                            dest.Double = a.Double % b.Double;
                            break;
                        }
                    }
                } break;
                case call : {
                    var index     = Readu32(bytes, ref pc);
                    var newPc     = GetFunctionByIndex(index);
                    var oldPc     = pc;
                    pc = newPc;
                    var regCount  = Readu16(bytes, ref pc);
                    var argCount  = Readu16(bytes, ref pc);
                    var regStart  = sf.RegEnd - (argCount - (uint)1); // is this a joke?
                    var regEnd    = regStart + regCount - 1;

                    sf = PushStackFrame(StackCurrent, oldPc, regStart, regEnd);
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
                case cmp : {
                    var regType = (RegType)Readu8(bytes, ref pc);
                    var r0      = Readu16(bytes, ref pc);
                    var r1      = Readu16(bytes, ref pc);
                    ref var reg0 = ref GetRegister(r0);
                    ref var reg1 = ref GetRegister(r1);
                    FLAGS = Flags.Uninitialized;

                    switch(regType) {
                        case RegType.s32 :
                            var v = reg0.s32 - reg1.s32;
                            if (v == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                        case RegType.u32 :
                            var v1 = reg0.u32 - reg1.u32; // fuck you v.1
                            if (v1 == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v1 > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v1 < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                        case RegType.s64 :
                            var v2 = reg0.s64 - reg1.s64; // fuck you v.2
                            if (v2 == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v2 > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v2 < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                        case RegType.u64 :
                            var v3 = reg0.u64 - reg1.u64; // fuck you v.3
                            if (v3 == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v3 > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v3 < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                        case RegType.Float :
                            var v4 = reg0.Float - reg1.Float; // fuck you v.4
                            if (v4 == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v4 > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v4 < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                        case RegType.Double :
                            var v5 = reg0.Double - reg1.Double; // fuck you v.5
                            if (v5 == 0) {
                                FLAGS |= Flags.Zero;
                                FLAGS |= Flags.EQ;
                            } else if (v5 > 0) {
                                FLAGS |= Flags.GT;
                            } else if (v5 < 0) {
                                FLAGS |= Flags.LT;
                            }
                            break;
                    }
                } break;

                case jmp: {
                    var index = Readu32(bytes, ref pc);
                    pc = GetLabelByIndex(index);
                } break;

                case jl: {
                    var index     = Readu32(bytes, ref pc);

                    if ((FLAGS & Flags.LT) == Flags.LT) pc = GetLabelByIndex(index);
                } break;

                case jg: {
                    var index = Readu32(bytes, ref pc);

                    if((FLAGS & Flags.GT) == Flags.GT) pc = GetLabelByIndex(index);
                } break;

                case je:{
                    var index = Readu32(bytes, ref pc);

                    if((FLAGS & Flags.Zero) == Flags.Zero) pc = GetLabelByIndex(index);
                } break;

                case jne:{
                    var index = Readu32(bytes, ref pc);

                    if((FLAGS & Flags.Zero) != Flags.Zero) pc = GetLabelByIndex(index);
                } break;

                case jz:{
                    var index = Readu32(bytes, ref pc);

                    if((FLAGS & Flags.Zero) == Flags.Zero) pc = GetLabelByIndex(index);
                } break;

                case jnz:{
                    var index = Readu32(bytes, ref pc);

                    if((FLAGS & Flags.Zero) != Flags.Zero) pc = GetLabelByIndex(index);
                } break;

                default: {
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

    public static float ReadFloat(byte[] bytes, ref uint ptr) {
        var i = Readu32(bytes, ref ptr);
        return *(float*)&i;
    }

    public static double ReadDouble(byte[] bytes, ref uint ptr) {
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

    private static uint GetFunctionByIndex(uint index) {
        Assert(index < FunctionTable.Length, $"Function index {index} is out of range {FunctionTable.Length}.");
        return FunctionTable[index];
    }

    private static uint GetLabelByIndex(uint index) {
        Assert(index < LabelTable.Length, $"Label index {index} is out of range {LabelTable.Length}.");
        return LabelTable[index];
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

    private static float ReadFloat(byte[] bytes, uint ptr) {
        var i = Readu32(bytes, ptr);
        return *(float*)&i;
    }

    private static double ReadDouble(byte[] bytes, uint ptr) {
        var i = Readu64(bytes, ptr);
        return *(double*)&i;
    }

    public static string BytecodeToString(byte[] bytes, uint count) {
        var  sb = new StringBuilder();
        uint pc = 4;

        // print function table
        var funCount = Readu32(bytes, ref pc);
        sb.Append("Functions count: ");
        sb.Append(funCount.ToString());
        sb.Append('\n');

        for (var i = 0; i < funCount; ++i) {
            var index = Readu32(bytes, ref pc);
            var pos   = Readu32(bytes, ref pc);

            sb.Append(index);
            sb.Append(':');
            sb.Append(pos);
            sb.Append('\n');
        }

        // print label table
        var labelCount = Readu32(bytes, ref pc);
        sb.Append("Labels count: ");
        sb.Append(labelCount.ToString());
        sb.Append('\n');

        for (var i = 0; i < labelCount; ++i) {
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
                case sub  :
                case mul  :
                case div  :
                case mod : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var regType = (RegType)Readu8(bytes, ref pc);
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

                case set : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var regType  = (RegType)Readu8(bytes, ref pc);
                    var reg      = Readu16(bytes, ref pc);
                    sb.Append(regType.ToString());
                    sb.Append(' ');
                    sb.Append(reg.ToString());
                    sb.Append(' ');

                    switch(regType) {
                        case RegType.s32 :
                            var constant = Reads32(bytes, ref pc);
                            sb.Append(constant.ToString());
                            break;
                        case RegType.u32 :
                            var constant1 = Readu32(bytes, ref pc); // fuck microsoft
                            sb.Append(constant1.ToString());
                            break;
                        case RegType.s64 :
                            var constant2 = Reads64(bytes, ref pc); // fuck microsoft
                            sb.Append(constant2.ToString());
                            break;
                        case RegType.u64 :
                            var constant3 = Readu64(bytes, ref pc); // fuck microsoft
                            sb.Append(constant3.ToString());
                            break;
                        case RegType.Float :
                            var constant4 = ReadFloat(bytes, ref pc); // fuck microsoft
                            sb.Append(constant4.ToString());
                            break;
                        case RegType.Double :
                            var constant5 = ReadDouble(bytes, ref pc); // fuck microsoft
                            sb.Append(constant5.ToString());
                            break;
                    }
                } break;

                case ret : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var reg = Readu16(bytes, ref pc);
                    sb.Append(reg.ToString());
                } break;

                case cmp : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var regType = (RegType)Readu8(bytes, ref pc);
                    var r0      = Readu16(bytes, ref pc);
                    var r1      = Readu16(bytes, ref pc);
                    sb.Append(regType.ToString());
                    sb.Append(' ');
                    sb.Append(r0.ToString());
                    sb.Append(' ');
                    sb.Append(r1.ToString());
                } break;

                case jmp : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var pos = Readu32(bytes, ref pc);
                    sb.Append(pos.ToString());
                } break;

                case jg  :
                case je  :
                case jne :
                case jz  :
                case jnz :
                case jl  : {
                    sb.Append(opcode.ToString());
                    sb.Append(' ');
                    var pos     = Readu32(bytes, ref pc);
                    sb.Append(pos.ToString());
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