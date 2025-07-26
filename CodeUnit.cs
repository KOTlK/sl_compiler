using System;
using System.Collections.Generic;

using static Opcode;
using static BytecodeConstants;

/*  Structure:
    Keyword 4B
    Functions Count 4B
    Function Table [index(4B) : position(4B)]
    Code...
*/
public unsafe class CodeUnit {
    public byte[]    Bytes;
    public uint      Count;
    public uint      Length;

    public CodeUnit(uint length) {
        Bytes    = new byte[length];
        Length   = length;
        Bytes[0] = 0x80;
        Bytes[1] = 0x00;
        Bytes[2] = 0x00;
        Bytes[3] = 0x8A;
        Count    = 4;
    }

    public void Push(Opcode a) {
        if (Count + 2 >= Length) {
            Resize((Count + 2) << 1);
        }

        var i = (ushort)a;

        Push(i);
    }

    public void Push(Opcode a, params int[] addData) {
        if (Count + 2 >= Length) {
            Resize((Count + 2) << 1);
        }

        var m = (ushort)a;

        Push(m);

        for (var i = 0; i < addData.Length; ++i) {
            Push(addData[i]);
        }
    }

    public void Pushset_s32(ushort dest, int val) {
        Push(set_s32);
        Push(dest);
        Push(val);
    }

    public void PushMath(Opcode instr, byte regType, ushort dest, ushort a, ushort b) {
        Push(instr);
        Push(regType);
        Push(dest);
        Push(a);
        Push(b);
    }

    public void PushMov(ushort dest, ushort src) {
        Push(mov);
        Push(dest);
        Push(src);
    }

    public void PushReturn(ushort reg) {
        Push(ret);
        Push(reg);
    }

    public uint PushFunction(ushort regCount, ushort argCount) {
        Push(func);
        var p = Count;
        Push(regCount);
        Push(argCount);

        return p;
    }

    public void PushCall(uint index) {
        Push(call);
        Push(index);
    }

    public uint PushMain(ushort regCount) {
        var p = PushFunction(regCount, 0);

        Bytes[MainPos]     = (byte)( p        & 0xFF);
        Bytes[MainPos + 1] = (byte)((p >> 8)  & 0xFF);
        Bytes[MainPos + 2] = (byte)((p >> 16) & 0xFF);
        Bytes[MainPos + 3] = (byte)((p >> 24) & 0xFF);

        return p;
    }

    public int GetMain() {
        var i = Bytes[MainPos]           |
                Bytes[MainPos + 1] << 8  |
                Bytes[MainPos + 2] << 16 |
                Bytes[MainPos + 3] << 24;

        return i;
    }

    public void Push(float f) {
        if (Count + 4 >= Length) {
            Resize((Count + 4) << 1);
        }

        uint *intPtr = (uint*)&f;
        uint intVal = *intPtr;

        Bytes[Count]     = (byte)( intVal        & 0xFF);
        Bytes[Count + 1] = (byte)((intVal >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((intVal >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((intVal >> 24) & 0xFF);

        Count += 4;
    }

    public void Push(double d) {
        if (Count + 8 >= Length) {
            Resize((Count + 8) << 1);
        }

        ulong *intPtr = (ulong*)&d;
        ulong intVal = *intPtr;

        Bytes[Count]     = (byte)( intVal        & 0xFF);
        Bytes[Count + 1] = (byte)((intVal >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((intVal >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((intVal >> 24) & 0xFF);
        Bytes[Count + 4] = (byte)((intVal >> 32) & 0xFF);
        Bytes[Count + 5] = (byte)((intVal >> 40) & 0xFF);
        Bytes[Count + 6] = (byte)((intVal >> 48) & 0xFF);
        Bytes[Count + 7] = (byte)((intVal >> 56) & 0xFF);

        Count += 8;
    }

    public void Push(int a) {
        if (Count + 4 >= Length) {
            Resize((Count + 4) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((a >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((a >> 24) & 0xFF);

        Count += 4;
    }

    public void Push(uint a) {
        if (Count + 4 >= Length) {
            Resize((Count + 4) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((a >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((a >> 24) & 0xFF);

        Count += 4;
    }

    public void Push(short a) {
        if (Count + 2 >= Length) {
            Resize((Count + 2) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);

        Count += 2;
    }

    public void Push(ushort a) {
        if (Count + 2 >= Length) {
            Resize((Count + 2) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);

        Count += 2;
    }

    public void Push(long a) {
        if (Count + 8 >= Length) {
            Resize((Count + 8) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((a >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((a >> 24) & 0xFF);
        Bytes[Count + 4] = (byte)((a >> 32) & 0xFF);
        Bytes[Count + 5] = (byte)((a >> 40) & 0xFF);
        Bytes[Count + 6] = (byte)((a >> 48) & 0xFF);
        Bytes[Count + 7] = (byte)((a >> 56) & 0xFF);

        Count += 8;
    }

    public void Push(ulong a) {
        if (Count + 8 >= Length) {
            Resize((Count + 8) << 1);
        }

        Bytes[Count]     = (byte)( a        & 0xFF);
        Bytes[Count + 1] = (byte)((a >> 8)  & 0xFF);
        Bytes[Count + 2] = (byte)((a >> 16) & 0xFF);
        Bytes[Count + 3] = (byte)((a >> 24) & 0xFF);
        Bytes[Count + 4] = (byte)((a >> 32) & 0xFF);
        Bytes[Count + 5] = (byte)((a >> 40) & 0xFF);
        Bytes[Count + 6] = (byte)((a >> 48) & 0xFF);
        Bytes[Count + 7] = (byte)((a >> 56) & 0xFF);

        Count += 8;
    }

    public void Push(byte b) {
        if (Count + 1 >= Length) {
            Resize((Count + 1) << 1);
        }

        Bytes[Count++] = b;
    }

    public void Push(sbyte b) {
        if (Count + 1 >= Length) {
            Resize((Count + 1) << 1);
        }

        Bytes[Count++] = (byte)b;
    }

    private uint Readu32(uint ptr) {
        var i = (uint)(Bytes[ptr]           |
                       Bytes[ptr + 1] << 8  |
                       Bytes[ptr + 2] << 16 |
                       Bytes[ptr + 3] << 24);

        return i;
    }

    public void SetFunctionPos(uint index, uint pos) {
        var offset = FunctionsOffset + 8 * index + 4;

        Bytes[offset]     = (byte)( pos        & 0xFF);
        Bytes[offset + 1] = (byte)((pos >> 8)  & 0xFF);
        Bytes[offset + 2] = (byte)((pos >> 16) & 0xFF);
        Bytes[offset + 3] = (byte)((pos >> 24) & 0xFF);
    }

    public void Resize(uint newSize) {
        var arr = new byte[newSize];

        for (var i = 0; i < Length; ++i) {
            arr[i] = Bytes[i];
        }

        Bytes  = arr;
        Length = newSize;
    }
}