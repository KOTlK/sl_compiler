using System.Collections.Generic;
using System.Text;

using static AstType;
using static TokenType;
using static Context;

public class Ast {
    public List<AstNode> Typedefs  = new List<AstNode>();
    public List<AstNode> Nodes     = new List<AstNode>();
    public List<AstNode> Functions = new List<AstNode>();

    public void Add(AstNode child) {
        Nodes.Add(child);
    }
}

public static class AstParser {
    public static Ast Parse(Lexer lexer) {
        TypeSystem.Init();
        var root = new Ast();
        lexer.EatToken();

        while (lexer.GetCurrent().Type != EndOfFile) {
            var currentToken = lexer.GetCurrent();
            switch (currentToken.Type) {
                case TokenType.Struct :
                    root.Typedefs.Add(ParseTypedef(lexer));
                    break;
                case TokenType.Ident : {
                    var next  = lexer.Peek();

                    if (next.Type == DoubleColon) {
                        root.Functions.Add(ParseFundef(lexer));
                    } else {
                        currentToken = lexer.EatToken();
                    }
                } break;
                default :
                    currentToken = lexer.GetCurrent();
                    Err.Push("unexpected symbol at %:%. Expected % or %, got %", currentToken.Line, currentToken.Column, TokenType.Struct, TokenType.Ident, currentToken.Type);
                    return null;
            }
        }

        return root;
    }

    // public static int ComputeExpression(AstNode expr) {
    //     switch (expr.Type) {
    //         case AstType.Number :
    //             return expr.IntValue;
    //         case AstType.Operator : {
    //             if(expr.IsBinary == false && expr.OperatorType == Minus) {
    //                 return -ComputeExpression(expr.Right);
    //             }

    //             var left  = ComputeExpression(expr.Left);
    //             var right = ComputeExpression(expr.Right);

    //             switch (expr.OperatorType) {
    //                 case Plus :
    //                     return left + right;
    //                 case Minus :
    //                     return left - right;
    //                 case Mul :
    //                     return left * right;
    //                 case Div :
    //                     return left / right;
    //                 case Mod :
    //                     return left % right;
    //                 default :
    //                     UnityEngine.Debug.LogError("WRONG");
    //                     break;
    //             }
    //         } break;
    //     }

    //     return 0;
    // }


    private static AstNode ParseAssignment(Lexer lexer) {
        var name = lexer.GetCurrent();
        if (AssertSymbol(name, TokenType.Ident)) return null;

        var ident = MakeIdent(name.StringValue);

        var equals = lexer.EatToken();
        if (AssertSymbol(equals, TokenType.Equals)) return null;

        lexer.EatToken();
        var assign = MakeAssign(ident, ParseExpression(lexer, -9999));

        if (AssertSymbol(lexer.EatToken(), Semicolon)) return null;
        lexer.EatToken();

        return assign;
    }

    private static AstNode ParseReturn(Lexer lexer) {
        var node      = new AstNode();
        node.Type     = StatementReturn;

        var token = lexer.EatToken();

        if (token.Type == Semicolon) {
            lexer.EatToken();
            return node;
        }

        node.Expression = ParseExpression(lexer, -9999);
        lexer.EatToken();

        if (AssertSymbol(lexer.GetCurrent(), Semicolon)) return null;
        lexer.EatToken();

        return node;
    }

    public static AstNode ParseExpression(Lexer lexer, int prec) {
        var token = lexer.GetCurrent();
        AstNode left = null;

        switch(token.Type) {
            case TokenType.Ident : {
                if (lexer.Peek().Type == ORParen) {
                    left = ParseFuncall(lexer);
                } else {
                    left = MakeIdent(token.StringValue);
                }
            } break;
            case TokenType.FloatLiteral  :
            case TokenType.DoubleLiteral :
            case TokenType.IntLiteral : {
                left = MakeNumericLiteral(token);
            } break;
            case TokenType.StringLiteral : {
                left = MakeLiteral(token, TypeSystem.String);
            } break;
            case TokenType.CharLiteral : {
                left = MakeLiteral(token, TypeSystem.Char);
            } break;
            case TokenType.ORParen : {
                lexer.EatToken();
                left = ParseExpression(lexer, -9999);
                lexer.EatToken();
            } break;
            case TokenType.Minus : {
                if(IsBinary(lexer) == false) {
                    var node          = new AstNode();
                    node.Type         = Operator;
                    node.OperatorType = TokenType.Minus;
                    node.IsBinary     = false;
                    var next = lexer.EatToken();
                    if (next.Type == TokenType.IntLiteral ||
                        next.Type == TokenType.FloatLiteral ||
                        next.Type == TokenType.DoubleLiteral) {
                        node.Right = MakeNumericLiteral(next);
                    } else if (next.Type == TokenType.StringLiteral) {
                        node.Right = MakeLiteral(next, TypeSystem.String);
                    } else if (next.Type == TokenType.CharLiteral) {
                        node.Right = MakeLiteral(next, TypeSystem.Char);
                    } else {
                        node.Right = ParseExpression(lexer, prec);
                    }
                    left = node;
                }
            } break;
        }

        while(true) {
            token         = lexer.EatToken();
            var tokenPrec = GetPrecedence(token.Type);

            if(IsOperator(token) == false) {
                lexer.TokensPtr--;
                break;
            }

            if(tokenPrec < prec) {
                lexer.TokensPtr--;
                break;
            }

            var node          = new AstNode();
            node.Type         = Operator;
            node.OperatorType = token.Type;
            node.IsBinary     = true;
            node.Left         = left;
            lexer.EatToken();
            node.Right        = ParseExpression(lexer, tokenPrec + 1);

            left = node;
        }

        return left;
    }

    private static AstNode ParseTypedef(Lexer lexer) {
        var name      = lexer.EatToken();
        var next      = lexer.EatToken();
        var node      = new AstNode();
        var type      = new TypeInfo();
        node.Type     = StatementTypedef;
        node.TypeInfo = type;
        type.Name     = name.StringValue;
        type.Fields   = new List<FieldInfo>();

        if (AssertSymbol(next, OParen)) return null;

        uint align = 1;
        uint size  = 0;

        while (next.Type != EndOfFile) {
            next          = lexer.EatToken();
            if (next.Type == CParen) break;

            if (AssertMultipleSymbol(next, TokenType.Ident, CParen)) return null;

            var colon     = lexer.EatToken();
            if (AssertSymbol(colon, Colon)) return null;

            var fieldType = lexer.EatToken();
            if (AssertSymbol(fieldType, TokenType.Ident)) return null;

            var semicolon = lexer.EatToken();
            if (AssertSymbol(semicolon, Semicolon)) return null;

            var field = new FieldInfo();

            field.Name = next.StringValue;
            field.Type = TypeSystem.GetType(fieldType.StringValue);

            if (field.Type.Align > align) {
                align = field.Type.Align;
            }

            size += field.Type.Size;

            type.Fields.Add(field);
        }

        if (AssertSymbol(lexer.GetCurrent(), CParen)) return null;
        lexer.EatToken();

        type.Align = align;
        type.Size  = size;

        var add = TypeSystem.RegisterType(type);

        if (!add) {
            Err.TypeAlreadyDefined(name.Line, name.Column, name.StringValue);
            return null;
        }

        return node;
    }

    private static AstNode ParseFundef(Lexer lexer) {
        var node      = new AstNode();
        node.Type     = StatementFundef;
        var name      = lexer.GetCurrent();
        node.Ident    = MakeIdent(name.StringValue);

        var c1        = lexer.EatToken();
        if (AssertSymbol(c1, DoubleColon)) return null;

        var next = lexer.EatToken();
        if (AssertSymbol(next, ORParen)) return null;
        if (AssertMultipleSymbol(lexer.Peek(), CRParen, TokenType.Ident)) return null;

        if (lexer.Peek().Type != CRParen) {
            node.Args = new List<AstNode>();
            // parse arguments
            while (next.Type != EndOfFile) {
                next = lexer.EatToken(); // next is ident

                if (next.Type == Comma) next = lexer.EatToken();
                if (next.Type == CRParen) break;
                if (AssertSymbol(next, TokenType.Ident)) return null;

                var colon = lexer.EatToken();
                if (AssertSymbol(colon, Colon)) return null;

                var ident       = MakeIdent(next.StringValue);
                AstNode  assign = null;
                TypeInfo type   = null;

                // @Incomplete suggest type of an argument
                if (lexer.Peek().Type == TokenType.Equals) {
                    lexer.EatToken();
                    lexer.EatToken();
                    assign = MakeAssign(ident, ParseExpression(lexer, -9999));
                    if (assign.Expression.Type == AstType.IntLiteral ||
                        assign.Expression.Type == AstType.FloatLiteral ||
                        assign.Expression.Type == AstType.DoubleLiteral ||
                        assign.Expression.Type == AstType.StringLiteral ||
                        assign.Expression.Type == AstType.CharLiteral) {
                        type = assign.Expression.TypeInfo;
                    }
                } else {
                    var t = lexer.EatToken();
                    if (AssertSymbol(t, TokenType.Ident)) return null;
                    type  = TypeSystem.GetType(t.StringValue);
                }

                var vardecl = MakeVar(ident, type, assign);

                node.Args.Add(vardecl);
            }
        } else {
            lexer.EatToken();
        }

        var cur = lexer.GetCurrent();
        if (AssertSymbol(cur, CRParen)) return null;

        var arrow = lexer.EatToken();

        if (arrow.Type == OParen) {
            node.TypeInfo = TypeSystem.Void;
        } else {
            if (AssertSymbol(arrow, ArrowRight)) return null;

            var tp = lexer.EatToken();
            if (AssertSymbol(tp, TokenType.Ident)) return null;

            node.TypeInfo = TypeSystem.GetType(tp.StringValue);
            lexer.EatToken();
        }

        node.Body = ParseBody(lexer);

        return node;
    }

    private static List<AstNode> ParseBody(Lexer lexer) {
        var nodes = new List<AstNode>();

        if (AssertSymbol(lexer.GetCurrent(), OParen)) return null;
        lexer.EatToken();

        while (lexer.GetCurrent().Type != EndOfFile) {
            var currentToken = lexer.GetCurrent();

            switch (currentToken.Type) {
                case CParen : {
                    lexer.EatToken();
                    return nodes;
                }
                case TokenType.Return : {
                    nodes.Add(ParseReturn(lexer));
                } break;
                case TokenType.Ident : {
                    var next = lexer.Peek();

                    if (next.Type == ORParen) {
                        nodes.Add(ParseFuncall(lexer));
                        var semicolon = lexer.EatToken();

                        if (AssertSymbol(semicolon, Semicolon)) return null;
                    } else if (next.Type == Colon) {
                        nodes.Add(ParseVardecl(lexer));
                    } else if (next.Type == TokenType.Equals) {
                        nodes.Add(ParseAssignment(lexer));
                    } else {
                        Err.Push("unexpected symbol at %:%. Expected % or % or % or %, got %.",
                                  next.Line,
                                  next.Column,
                                  TokenType.Colon,
                                  TokenType.Equals,
                                  TokenType.CParen,
                                  "Statement",
                                  next.Type);
                        return null;
                    }
                    break;
                }
                default : {
                    Err.Push("unexpected symbol at %:%. Expected % or % or %, got %.",
                              currentToken.Line,
                              currentToken.Column,
                              TokenType.Return,
                              TokenType.Ident,
                              TokenType.CParen,
                              currentToken.Type);
                    return null;
                }
            }
        }

        if (AssertSymbol(lexer.GetCurrent(), CParen)) return null;
        lexer.EatToken();

        return nodes;
    }

    private static AstNode ParseVardecl(Lexer lexer) {
        var name = lexer.GetCurrent();
        if (AssertSymbol(name, TokenType.Ident)) return null;

        var ident = MakeIdent(name.StringValue);
        var next  = lexer.EatToken();
        if (AssertSymbol(next, TokenType.Colon)) return null;

        next = lexer.EatToken();

        if (next.Type == TokenType.Equals) {
            // a := 10;
            lexer.EatToken();
            var assign  = MakeAssign(ident, ParseExpression(lexer, -9999));
            var vardecl = MakeVar(ident, assign.Expression.TypeInfo, assign);

            if (AssertSymbol(lexer.EatToken(), Semicolon)) return null;
            lexer.EatToken();

            return vardecl;
        } else if (next.Type == TokenType.Ident) {
            // a : s32 = 10;
            // or
            // a : s32;
            var type = TypeSystem.GetType(next.StringValue);

            next = lexer.EatToken();

            if (next.Type == Semicolon) {
                // a : s32;
                lexer.EatToken();
                return MakeVar(ident, type);
            } else if (next.Type == TokenType.Equals) {
                // a : s32 = 10;
                lexer.EatToken();
                var assign  = MakeAssign(ident, ParseExpression(lexer, -9999));
                var vardecl = MakeVar(ident, type, assign);

                if (AssertSymbol(lexer.EatToken(), Semicolon)) return null;
                lexer.EatToken();

                return vardecl;
            } else {
                Err.Push("unexpected symbol at %:%. Expected % or %, got %", next.Line,
                                                                             next.Column,
                                                                             TokenType.Semicolon,
                                                                             TokenType.Equals,
                                                                             next.Type);
                return null;
            }
        } else {
            Err.Push("unexpected symbol at %:%. Expected % or %, got %", next.Line,
                                                                         next.Column,
                                                                         TokenType.Equals,
                                                                         TokenType.Ident,
                                                                         next.Type);
            return null;
        }
    }

    private static AstNode ParseFuncall(Lexer lexer) {
        var name      = lexer.GetCurrent();
        var node      = new AstNode();
        node.Type     = StatementFuncall;
        node.Ident    = MakeIdent(name.StringValue);

        lexer.EatToken(); // ORParen 100% here
        var cparen = lexer.EatToken();

        if (cparen.Type == CRParen) {
            lexer.EatToken();

            var semicolon = lexer.EatToken();
            if (AssertSymbol(semicolon, Semicolon)) return null;

            return node;
        }

        node.Args = new List<AstNode>();

        while (lexer.GetCurrent().Type != EndOfFile) {
            var arg  = ParseExpression(lexer, -9999);
            var next = lexer.EatToken();

            node.Args.Add(arg);

            if (next.Type == CRParen) break;
            if (AssertSymbol(next, Comma)) return null;

            lexer.EatToken();
        }

        cparen = lexer.GetCurrent();
        if (AssertSymbol(cparen, CRParen)) return null;

        return node;
    }

    private static AstNode MakeIdent(string ident) {
        var node    = new AstNode();
        node.Type   = AstType.Ident;
        node.String = ident;

        return node;
    }

    private static AstNode MakeLiteral(Token token, TypeInfo type = null) {
        var node    = new AstNode();
        node.Type   = AstType.StringLiteral;
        node.String = token.StringValue;

        if(type == null) {
            node.TypeInfo = GuessLiteralType(token.StringValue);
        } else {
            node.TypeInfo = type;
        }

        return node;
    }

    private static AstNode MakeNumericLiteral(Token token) {
        var node    = new AstNode();

        switch(token.Type) {
            case TokenType.IntLiteral :
                node.Type = AstType.IntLiteral;
                node.Number.IntValue = token.LiteralValue.IntValue;
                node.TypeInfo        = GuessIntType(token.LiteralValue.IntValue);
                break;
            case TokenType.FloatLiteral :
                node.Type = AstType.FloatLiteral;
                node.Number.FloatValue = token.LiteralValue.FloatValue;
                node.TypeInfo          = TypeSystem.Float;
                break;
            case TokenType.DoubleLiteral :
                node.Type = AstType.DoubleLiteral;
                node.Number.DoubleValue = token.LiteralValue.DoubleValue;
                node.TypeInfo           = TypeSystem.Double;
                break;
        }

        return node;
    }

    private static AstNode MakeVar(AstNode ident, TypeInfo type, AstNode assign = null) {
        var vardecl      = new AstNode();
        vardecl.Type     = StatementVardecl;
        vardecl.Ident    = ident;
        vardecl.TypeInfo = type;
        vardecl.Stmt     = assign;

        return vardecl;
    }

    private static AstNode MakeAssign(AstNode ident, AstNode expr) {
        var assign        = new AstNode();
        assign.Type       = StatementAssign;
        assign.Ident      = ident;
        assign.Expression = expr;

        return assign;
    }

    private static bool IsBinary(Lexer lexer) {
        var cur  = lexer.GetCurrent();

        switch(cur.Type) {
            case Plus    : return true;
            case Minus   : {
                if(lexer.TokensPtr == 0) return false;
                var prev = lexer.Previous();
                if(IsOperator(prev) ||
                   prev.Type == TokenType.Equals ||
                   prev.Type == ORParen) {
                    return false;
                }
                return true;
            }
            case Mul     : return true;
            case Div     : return true;
            case Exp     : return true;
            case Mod     : return true;
            case ORParen : return true;

            default : return false;
        }
    }

    private static bool IsOperator(Token token) {
        switch(token.Type) {
            case Minus : return true;
            case Plus  : return true;
            case Mul   : return true;
            case Div   : return true;
            case Exp   : return true;
            case Mod   : return true;
            default    : return false;
        }
    }

    private static int GetPrecedence(TokenType token) {
        switch (token) {
            case Minus : return 10;
            case Plus  : return 10;
            case Mul   : return 20;
            case Div   : return 20;
            case Mod   : return 20;
            case Exp   : return 30;
            default    : return 0;
        }
    }

    private static TypeInfo GuessIntType(ulong num) {
        if (num < 128)                 return TypeSystem.s8;
        if (num < 256)                 return TypeSystem.u8;
        if (num < 32768)               return TypeSystem.s16;
        if (num < 65536)               return TypeSystem.u16;
        if (num < 2147483648)          return TypeSystem.s32;
        if (num < 4294967296)          return TypeSystem.u32;
        if (num < 9223372036854775808) return TypeSystem.s64;

        return TypeSystem.u64;
    }

    private static TypeInfo GuessLiteralType(string value) {
        if (value[value.Length - 1] == 'f' ||
           value[value.Length - 1] == 'F') {
            return TypeSystem.Float;
        }

        if (value[value.Length - 1] == 'd' ||
           value[value.Length - 1] == 'D') {
            return TypeSystem.Double;
        }

        if (float.TryParse(value, out var f)) {
            return TypeSystem.Float;
        }

        if (double.TryParse(value, out var d)) {
            return TypeSystem.Double;
        }

        if (byte.TryParse(value, out var u8)) {
            return TypeSystem.u8;
        }

        if (sbyte.TryParse(value, out var s8)) {
            return TypeSystem.s8;
        }

        if (short.TryParse(value, out var s16)) {
            return TypeSystem.s16;
        }

        if (ushort.TryParse(value, out var u16)) {
            return TypeSystem.u16;
        }

        if (int.TryParse(value, out var s32)) {
            return TypeSystem.s32;
        }

        if (uint.TryParse(value, out var u32)) {
            return TypeSystem.u32;
        }

        if (long.TryParse(value, out var s64)) {
            return TypeSystem.s64;
        }

        if (ulong.TryParse(value, out var u64)) {
            return TypeSystem.u64;
        }

        return null;
    }

    private static bool AssertSymbol(Token token, TokenType type) {
        if (token.Type != type) {
            UnexpectedSymbol(token.Line, token.Column, type, token.Type);
            return true;
        }

        return false;
    }

    private static StringBuilder AssertSb = new StringBuilder();
    private static bool AssertMultipleSymbol(Token token, params TokenType[] types) {
        for (var i = 0; i < types.Length; ++i) {
            if (token.Type == types[i]) {
                return false;
            }
        }

        AssertSb.Clear();

        AssertSb.Append("unexpected symbol at ");
        AssertSb.Append(token.Line.ToString());
        AssertSb.Append(':');
        AssertSb.Append(token.Column.ToString());
        AssertSb.Append(". Expected ");
        AssertSb.Append(types[0]);

        for (var i = 1; i < types.Length; ++i) {
            AssertSb.Append(" or ");
            AssertSb.Append(types[i].ToString());
        }

        AssertSb.Append(", got ");
        AssertSb.Append(token.Type.ToString());
        AssertSb.Append('.');

        Err.Push(AssertSb.ToString());

        return true;
    }

    private static void UnexpectedSymbol(int line, int column, TokenType expected, TokenType got) {
        Err.Push($"unexpected symbol at {line}:{column}. Expected {expected}, got {got}");
    }
}