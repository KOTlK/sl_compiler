using System.Runtime.InteropServices;

public enum RegisterType : byte {
    Uninitialized = 0,
    u8      = 1,
    s8      = 2,
    u16     = 3,
    s16     = 4,
    u32     = 5,
    s32     = 6,
    u64     = 7,
    s64     = 8,
    Float   = 9,
    Double  = 10,
    Pointer = 11,
    Data    = 12,
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Register {
    [FieldOffset(0)] public RegisterType Type;
    [FieldOffset(1)] public ushort    CustomSize;
    [FieldOffset(8)] public byte      u8;
    [FieldOffset(8)] public sbyte     s8;
    [FieldOffset(8)] public ushort    u16;
    [FieldOffset(8)] public short     s16;
    [FieldOffset(8)] public uint      u32;
    [FieldOffset(8)] public int       s32;
    [FieldOffset(8)] public ulong     u64;
    [FieldOffset(8)] public long      s64;
    [FieldOffset(8)] public float     Float;
    [FieldOffset(8)] public double    Double;
    [FieldOffset(8)] public uint      Pointer;
    [FieldOffset(8)] public uint      StackStart;

    public static Register Makeu8(byte n) {
        var v  = new Register();
        v.Type = RegisterType.u8;
        v.u8   = n;
        return v;
    }

    public static Register Makes8(sbyte n) {
        var v  = new Register();
        v.Type = RegisterType.s8;
        v.s8   = n;
        return v;
    }

    public static Register Makeu16(ushort n) {
        var v  = new Register();
        v.Type = RegisterType.u16;
        v.u16  = n;
        return v;
    }

    public static Register Makes16(short n) {
        var v  = new Register();
        v.Type = RegisterType.s16;
        v.s16  = n;
        return v;
    }

    public static Register Makeu32(uint n) {
        var v  = new Register();
        v.Type = RegisterType.u32;
        v.u32  = n;
        return v;
    }

    public static Register Makes32(int n) {
        var v  = new Register();
        v.Type = RegisterType.s32;
        v.s32  = n;
        return v;
    }

    public static Register Makeu64(ulong n) {
        var v  = new Register();
        v.Type = RegisterType.u64;
        v.u64  = n;
        return v;
    }

    public static Register Makes64(long n) {
        var v  = new Register();
        v.Type = RegisterType.s64;
        v.s64  = n;
        return v;
    }

    public static Register MakeFloat(float n) {
        var v   = new Register();
        v.Type  = RegisterType.Float;
        v.Float = n;
        return v;
    }

    public static Register MakeDouble(double n) {
        var v    = new Register();
        v.Type   = RegisterType.Double;
        v.Double = n;
        return v;
    }

    public static Register MakeCustom(ushort size, uint stackStart) {
        var v        = new Register();
        v.Type       = RegisterType.Data;
        v.CustomSize = size;
        v.StackStart = stackStart;
        return v;
    }

    public static Register MakePointer(uint n) {
        var v     = new Register();
        v.Type    = RegisterType.Pointer;
        v.Pointer = n;
        return v;
    }
}