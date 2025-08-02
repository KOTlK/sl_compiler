using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Register {
    [FieldOffset(0)] public byte      u8;
    [FieldOffset(0)] public sbyte     s8;
    [FieldOffset(0)] public ushort    u16;
    [FieldOffset(0)] public short     s16;
    [FieldOffset(0)] public uint      u32;
    [FieldOffset(0)] public int       s32;
    [FieldOffset(0)] public ulong     u64;
    [FieldOffset(0)] public long      s64;
    [FieldOffset(0)] public float     Float;
    [FieldOffset(0)] public double    Double;
    [FieldOffset(0)] public uint      Pointer;

    public static Register Makeu8(byte n) {
        var v  = new Register();
        v.u8   = n;
        return v;
    }

    public static Register Makes8(sbyte n) {
        var v  = new Register();
        v.s8   = n;
        return v;
    }

    public static Register Makeu16(ushort n) {
        var v  = new Register();
        v.u16  = n;
        return v;
    }

    public static Register Makes16(short n) {
        var v  = new Register();
        v.s16  = n;
        return v;
    }

    public static Register Makeu32(uint n) {
        var v  = new Register();
        v.u32  = n;
        return v;
    }

    public static Register Makes32(int n) {
        var v  = new Register();
        v.s32  = n;
        return v;
    }

    public static Register Makeu64(ulong n) {
        var v  = new Register();
        v.u64  = n;
        return v;
    }

    public static Register Makes64(long n) {
        var v  = new Register();
        v.s64  = n;
        return v;
    }

    public static Register MakeFloat(float n) {
        var v   = new Register();
        v.Float = n;
        return v;
    }

    public static Register MakeDouble(double n) {
        var v    = new Register();
        v.Double = n;
        return v;
    }

    public static Register MakePointer(uint n) {
        var v     = new Register();
        v.Pointer = n;
        return v;
    }
}