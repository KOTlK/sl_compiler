using System;
using System.Collections.Generic;

using static AstType;
using static Opcode;
using static TypeSystem;
using static Context;

public static class BytecodeConverter {
    public struct Function {
        public byte         Index;
        public uint         Position;
        public ushort       RegCount;
        public ushort       ArgCount;
        public Dictionary<string, ushort> Vars;

        public Function(byte index, uint pos) {
            Index       = index;
            Position    = pos;
            RegCount    = 0;
            ArgCount    = 0;
            Vars        = DictionaryPool<string, ushort>.Get();
        }

        public void Free() {
            DictionaryPool<string, ushort>.Release(Vars);
        }
    }

    public static CodeUnit AstToBytecode(Ast ast) {
        var cu       = new CodeUnit(2048);
        var funcs    = DictionaryPool<string, Function>.Get();
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

            if (func.Args != null) {
                f.ArgCount = (ushort)func.Args.Count;
                foreach (var arg in func.Args) {
                    f.Vars.Add(arg.Ident.String, f.RegCount++);
                }
            }

            foreach(var node in func.Body) {
                switch(node.Type) {
                    case StatementVardecl : {
                        f.Vars.Add(node.Ident.String, f.RegCount++);
                    } break;
                }
            }

            funcs.Add(name, f);
        }

        // Second pass. Convert function to bytecode
        foreach (var func in ast.Functions) {
            var name    = func.Ident.String;
            var f       = funcs[name];

            var pos     = cu.PushFunction(f.RegCount, f.ArgCount);
            f.Position  = pos;
            funcs[name] = f;
            cu.SetFunctionPos(f.Index, f.Position);

            if (func.Body == null)  {
                cu.PushReturn(0);
                continue;
            }

            var lastReturn = false;

            foreach (var node in func.Body) {
                switch (node.Type) {
                    case StatementVardecl : {
                        if (node.Stmt == null) break;

                        var varName = node.Ident.String;
                        var assign  = node.Stmt;
                        var reg     = f.Vars[varName];
                        ExpressionToBytecode(reg, f.RegCount, cu, assign.Expression, f.Vars);
                    } break;
                    case StatementAssign : {
                        var varName = node.Ident.String;
                        var reg     = f.Vars[varName];
                        ExpressionToBytecode(reg, f.RegCount, cu, node.Expression, f.Vars);
                    } break;
                    case StatementReturn : {
                        if (node.Expression != null) {
                            ExpressionToBytecode(0, f.RegCount, cu, node.Expression, f.Vars);
                        }
                        cu.PushReturn(0);
                        lastReturn = true;
                    } break;
                }
            }

            if (!lastReturn) {
                cu.PushReturn(0);
            }
        }

        foreach(var a in funcs) {
            a.Value.Free();
        }

        DictionaryPool<string, Function>.Release(funcs);

        return cu;
    }

    private static ushort ExpressionToBytecode(ushort result, ushort addReg, CodeUnit cu, AstNode expr, Dictionary<string, ushort> vars) {
        switch(expr.Type) {
            case Operator : {
                // addReg + 1 = int, while addReg is ushort. FUCK YOU.
                var a = ExpressionToBytecode(addReg, (ushort)(addReg + 1), cu, expr.Left, vars);
                var b = ExpressionToBytecode((ushort)(addReg + 1), (ushort)(addReg + 2), cu, expr.Right, vars);

                // @Incomplete
                switch(expr.OperatorType) {
                    case TokenType.Plus :
                        cu.PushMath(add, 0, result, a, b);
                        break;
                    case TokenType.Minus :
                        cu.PushMath(sub, 0, result, a, b);
                        break;
                    case TokenType.Mul :
                        cu.PushMath(mul, 0, result, a, b);
                        break;
                    case TokenType.Div :
                        cu.PushMath(div, 0, result, a, b);
                        break;
                    case TokenType.Mod :
                        cu.PushMath(mod, 0, result, a, b);
                        break;
                }
            } break;
            case IntLiteral : {
                cu.Pushset_s32(result, (int)expr.Number.IntValue);
            } break;
            case Ident : {
                if (vars.ContainsKey(expr.String)) {
                    return vars[expr.String];
                } else {
                    Err.Push("Undeclared identifier % at %:%", expr.String, expr.Line, expr.Column);
                }
            } break;
        }

        return result;
    }
}