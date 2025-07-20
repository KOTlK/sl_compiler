using System;
using System.Collections.Generic;

using static AstType;
using static Opcode;
using static TypeSystem;
using static Context;

public static class BytecodeConverter {
    public struct Function {
        public byte Index;
        public uint Position;
        public byte ArgsCount;
        public byte LocalsCount;
        public List<ushort> ArgsLocals;

        public Function(byte index, uint pos) {
            Index       = index;
            Position    = pos;
            ArgsCount   = 0;
            LocalsCount = 0;
            ArgsLocals  = new List<ushort>();
        }
    }

    public static CodeUnit AstToBytecode(Ast ast) {
        var cu       = new CodeUnit(2048);
        var funcs    = DictionaryPool<string, Function>.Get();
        var vars     = DictionaryPool<string, byte>.Get();
        var args     = DictionaryPool<string, byte>.Get();
        cu.Push(ast.Functions.Count);
        byte funcIndex = 1;
        // push main
        cu.Push((uint)0);
        cu.Push((uint)0);

        // First pass. Assign indices, gather sizes
        foreach (var func in ast.Functions) {
            var name = func.Ident.String;
            Function f;

            if (name == "main") {
                f = new Function(0, 0);
            } else {
                cu.Push((uint)funcIndex);
                cu.Push((uint)0);
                f = new Function(funcIndex, 0);
                funcIndex++;
            }

            if (func.Args == null) {
                f.ArgsCount = 0;
            } else {
                f.ArgsCount = (byte)func.Args.Count;
                foreach (var arg in func.Args) {
                    args.Add(arg.Ident.String, (byte)f.ArgsLocals.Count);
                    f.ArgsLocals.Add((ushort)arg.TypeInfo.Size);
                }
            }

            foreach(var node in func.Body) {
                switch(node.Type) {
                    case StatementVardecl : {
                        vars.Add(node.Ident.String, (byte)f.ArgsLocals.Count);
                        f.ArgsLocals.Add((ushort)node.TypeInfo.Size);
                        f.LocalsCount++;
                    } break;
                }
            }

            funcs.Add(name, f);
        }

        // Second pass. Convert function to bytecode
        foreach (var func in ast.Functions) {
            var name = func.Ident.String;
            var retSize = func.TypeInfo.Size;
            var f       = funcs[name];

            var pos = cu.PushFunction(f.ArgsCount, f.LocalsCount, retSize, f.ArgsLocals);
            f.Position = pos;
            funcs[name] = f;
            cu.SetFunctionPos(f.Index, f.Position);
            // cu.Push(ret);

            if (func.Body == null) continue;

            foreach (var node in func.Body) {
                switch (node.Type) {
                    case StatementVardecl : {
                        if (node.Stmt == null) break;

                        var varName = node.Ident.String;
                        var assign  = node.Stmt;
                        var v       = vars[varName];
                        ExpressionToBytecode(cu, assign.Expression, vars, args);
                        cu.Pushslocal(v);
                    } break;
                    case StatementAssign : {
                        var varName = node.Ident.String;
                        ExpressionToBytecode(cu, node.Expression, vars, args);

                        if (vars.ContainsKey(varName)) {
                            var v = vars[varName];
                            cu.Pushslocal(v);
                        }
                    } break;
                    case StatementReturn : {
                        if (node.Expression != null) {
                            ExpressionToBytecode(cu, node.Expression, vars, args);
                        }
                        cu.Push(ret);
                    } break;
                }
            }
        }

        return cu;
    }

    private static void ExpressionToBytecode(CodeUnit cu, AstNode expr, Dictionary<string, byte> vars, Dictionary<string, byte> args) {
        switch(expr.Type) {
            case Operator : {
                ExpressionToBytecode(cu, expr.Left, vars, args);
                // @Incomplete
                ExpressionToBytecode(cu, expr.Right, vars, args);
                cu.Push(add_s32);
            } break;
            case IntLiteral : {
                cu.Push(push_s32, (int)expr.Number.IntValue);
            } break;
            case Ident : {
                if (vars.ContainsKey(expr.String)) {
                    var v = vars[expr.String];
                    cu.Pushllocal(v);
                } else if (args.ContainsKey(expr.String)) {
                    var v = args[expr.String];
                    cu.Pushlarg(v);
                }
            } break;
        }
    }
}