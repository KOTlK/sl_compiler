using System.Collections.Generic;

public class TypeInfo {
    public string          Name;
    public uint            Align;
    public uint            Size;
    public List<FieldInfo> Fields;
}

public struct FieldInfo {
    public string   Name;
    public TypeInfo Type;
}

public static class TypeSystem {
    public static Dictionary<string, TypeInfo> Types = new Dictionary<string, TypeInfo>();

    public static readonly TypeInfo u8 = new TypeInfo() {
        Name   = "u8",
        Align  = 1,
        Size   = 1,
        Fields = null,
    };

    public static readonly TypeInfo s8 = new TypeInfo() {
        Name   = "s8",
        Align  = 1,
        Size   = 1,
        Fields = null,
    };

    public static readonly TypeInfo u16 = new TypeInfo() {
        Name   = "u16",
        Align  = 2,
        Size   = 2,
        Fields = null,
    };

    public static readonly TypeInfo s16 = new TypeInfo() {
        Name   = "s16",
        Align  = 2,
        Size   = 2,
        Fields = null,
    };

    public static readonly TypeInfo u32 = new TypeInfo() {
        Name   = "u32",
        Align  = 4,
        Size   = 4,
        Fields = null,
    };

    public static readonly TypeInfo s32 = new TypeInfo() {
        Name   = "s32",
        Align  = 4,
        Size   = 4,
        Fields = null,
    };

    public static readonly TypeInfo u64 = new TypeInfo() {
        Name   = "u64",
        Align  = 8,
        Size   = 8,
        Fields = null,
    };

    public static readonly TypeInfo s64 = new TypeInfo() {
        Name   = "s64",
        Align  = 8,
        Size   = 8,
        Fields = null,
    };

    public static readonly TypeInfo Float = new TypeInfo() {
        Name   = "float",
        Align  = 4,
        Size   = 4,
        Fields = null,
    };

    public static readonly TypeInfo Double = new TypeInfo() {
        Name   = "double",
        Align  = 8,
        Size   = 8,
        Fields = null,
    };

    public static readonly TypeInfo Char = new TypeInfo() {
        Name   = "char",
        Align  = 2,
        Size   = 2,
        Fields = null,
    };

    public static readonly TypeInfo String = new TypeInfo() {
        Name   = "string",
        Align  = 4,
        Size   = 4,
        Fields = null,
    };

    public static readonly TypeInfo Void = new TypeInfo() {
        Name   = "void",
        Align  = 0,
        Size   = 0,
        Fields = null,
    };

    public static void Init() {
        Types.Clear();
    }

    public static bool RegisterType(TypeInfo type) {
        if (Types.ContainsKey(type.Name)) {
            return false;
        }

        Types.Add(type.Name, type);
        return true;
    }

    public static TypeInfo GetType(string name) {
        if(IsPrimitive(name)) {
            return GetPrimitive(name);
        }

        return Types[name];
    }

    public static bool IsPrimitive(TypeInfo type) {
        return IsPrimitive(type.Name);
    }

    public static bool IsPrimitive(string typeName) {
        switch(typeName) {
            case "u8"     : return true;
            case "s8"     : return true;
            case "u16"    : return true;
            case "s16"    : return true;
            case "s32"    : return true;
            case "u32"    : return true;
            case "u64"    : return true;
            case "s64"    : return true;
            case "float"  : return true;
            case "double" : return true;
            case "string" : return true;
            case "char"   : return true;
            case "void"   : return true;
            default       : return false;
        }
    }

    public static TypeInfo GetPrimitive(string name) {
        switch(name) {
            case "u8"     : return u8;
            case "s8"     : return s8;
            case "u16"    : return u16;
            case "s16"    : return s16;
            case "s32"    : return s32;
            case "u32"    : return u32;
            case "u64"    : return u64;
            case "s64"    : return s64;
            case "float"  : return Float;
            case "double" : return Double;
            case "string" : return String;
            case "char"   : return Char;
            case "void"   : return Void;
            default       : return null;
        }
    }
}