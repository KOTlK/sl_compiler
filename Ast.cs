using System.Collections.Generic;

using static AstType;
using static TokenType;
using static StatementType;

public class Ast {
    public List<AstNode> Typedefs  = new List<AstNode>();
    public List<AstNode> Nodes     = new List<AstNode>();
    public List<AstNode> Functions = new List<AstNode>();

    public void Add(AstNode child) {
        Nodes.Add(child);
    }
}

public static class AstParser {
    public static Ast Parse(Tokenizer tokenizer, ErrorStream err) {
        TypeSystem.Init();
        var root = new Ast();

        while (tokenizer.GetCurrent().Type != EndOfFile) {
            var currentToken = tokenizer.GetCurrent();
            switch (currentToken.Type) {
                case TokenType.Equals :
                    root.Add(ParseAssignment(tokenizer, err));
                    break;
                case TokenType.Return :
                    root.Add(ParseReturn(tokenizer, err));
                    break;
                case TokenType.Struct :
                    root.Typedefs.Add(ParseTypedef(tokenizer, err));
                    break;
                case TokenType.Ident : {
                    var next  = tokenizer.Peek();
                    var next2 = tokenizer.Peek(2);

                    if (next.Type == Colon && next2.Type == Colon) {
                        root.Functions.Add(ParseFundef(tokenizer, err));
                    }
                } break;
                default :
                    currentToken = tokenizer.EatToken();
                    break;
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


    private static AstNode ParseAssignment(Tokenizer tokenizer, ErrorStream err) {
        var prev  = tokenizer.Previous();
        // var next  = tokenizer.Peek();
        var ident = prev.Type == TokenType.Ident ? prev : tokenizer.Previous(2);

        // ident = expr; == assign
        tokenizer.EatToken();
        var assign = MakeAssign(MakeIdent(ident.StringValue),
                                ParseExpression(tokenizer, err, -9999));

        if(prev.Type == Colon) {
            // @Incomplete suggest type of the expression;
            // ident := expr; == vardecl + assign
            var vardecl = MakeVar(assign.Ident, null, assign);

            assign = vardecl;
        }

        return assign;
    }

    private static AstNode ParseReturn(Tokenizer tokenizer, ErrorStream err) {
        var node      = new AstNode();
        node.Type     = Statement;
        node.StmtType = StatementType.Return;

        var token = tokenizer.EatToken();

        if (token.Type == Semicolon) {
            tokenizer.EatToken();
            return node;
        }

        node.Expression = ParseExpression(tokenizer, err, -9999);
        tokenizer.EatToken();

        if (AssertSymbol(tokenizer.GetCurrent(), Semicolon, err)) return null;

        return node;
    }

    public static AstNode ParseExpression(Tokenizer tokenizer, ErrorStream err, int prec) {
        var token = tokenizer.GetCurrent();
        AstNode left = null;

        switch(token.Type) {
            case TokenType.Ident : {
                if (tokenizer.Peek().Type == ORParen) {
                    left = ParseFuncall(tokenizer, err);
                } else {
                    left = MakeIdent(token.StringValue);
                }
            } break;
            case TokenType.Literal : {
                left = MakeLiteral(token);
            } break;
            case TokenType.StringLiteral : {
                left = MakeLiteral(token, TypeSystem.String);
            } break;
            case TokenType.CharLiteral : {
                left = MakeLiteral(token, TypeSystem.Char);
            } break;
            case TokenType.ORParen : {
                tokenizer.EatToken();
                left = ParseExpression(tokenizer, err, -9999);
                tokenizer.EatToken();
            } break;
            case TokenType.Minus : {
                if(IsBinary(tokenizer) == false) {
                    var node          = new AstNode();
                    node.Type         = Operator;
                    node.OperatorType = TokenType.Minus;
                    node.IsBinary     = false;
                    var next = tokenizer.EatToken();
                    if (next.Type == TokenType.Literal) {
                        node.Right = MakeLiteral(next);
                    } else if (next.Type == TokenType.StringLiteral) {
                        node.Right = MakeLiteral(next, TypeSystem.String);
                    } else if (next.Type == TokenType.CharLiteral) {
                        node.Right = MakeLiteral(next, TypeSystem.Char);
                    } else {
                        node.Right = ParseExpression(tokenizer, err, prec);
                    }
                    left = node;
                }
            } break;
        }

        while(true) {
            token         = tokenizer.EatToken();
            var tokenPrec = GetPrecedence(token.Type);

            if(IsOperator(token) == false) {
                tokenizer.Current--;
                break;
            }

            if(tokenPrec < prec) {
                tokenizer.Current--;
                break;
            }

            var node          = new AstNode();
            node.Type         = Operator;
            node.OperatorType = token.Type;
            node.IsBinary     = true;
            node.Left         = left;
            tokenizer.EatToken();
            node.Right        = ParseExpression(tokenizer, err, tokenPrec + 1);

            left = node;
        }

        return left;
    }

    private static AstNode ParseTypedef(Tokenizer tokenizer, ErrorStream err) {
        var name      = tokenizer.EatToken();
        var next      = tokenizer.EatToken();
        var node      = new AstNode();
        var type      = new TypeInfo();
        node.Type     = Statement;
        node.StmtType = Typedef;
        node.TypeInfo = type;
        type.Name     = name.StringValue;
        type.Fields   = new List<FieldInfo>();

        if (next.Type != OParen) {
            err.UnexpectedSymbol(next.Line, next.Column, OParen, next.Type);
            return null;
        }

        uint align = 1;
        uint size  = 0;

        while (next.Type != EndOfFile) {
            next          = tokenizer.EatToken();
            if (next.Type == CParen) break;

            if (next.Type != TokenType.Ident) {
                err.UnexpectedSymbol(next.Line, next.Column, TokenType.Ident, next.Type);
                return null;
            }

            var colon     = tokenizer.EatToken();

            if (colon.Type != Colon) {
                err.UnexpectedSymbol(colon.Line, colon.Column, Colon, colon.Type);
                return null;
            }

            var fieldType = tokenizer.EatToken();

            if (fieldType.Type != TokenType.Ident) {
                err.UnexpectedSymbol(fieldType.Line, fieldType.Column, TokenType.Ident, fieldType.Type);
                return null;
            }

            var semicolon = tokenizer.EatToken();

            if (semicolon.Type != Semicolon) {
                err.UnexpectedSymbol(semicolon.Line, semicolon.Column, Semicolon, semicolon.Type);
                return null;
            }

            var field = new FieldInfo();

            field.Name = next.StringValue;
            field.Type = TypeSystem.GetType(fieldType.StringValue);

            if (field.Type.Align > align) {
                align = field.Type.Align;
            }

            size += field.Type.Size;

            type.Fields.Add(field);

        }

        if(next.Type == EndOfFile) {
            err.UnexpectedSymbol(next.Line, next.Column, CParen, EndOfFile);
        }

        type.Align = align;
        type.Size  = size;

        var add = TypeSystem.RegisterType(type);

        if (!add) {
            err.TypeAlreadyDefined(name.Line, name.Column, name.StringValue);
            return null;
        }

        return node;
    }

    private static AstNode ParseFundef(Tokenizer tokenizer, ErrorStream err) {
        var node      = new AstNode();
        node.Type     = Statement;
        node.StmtType = Fundef;
        var name      = tokenizer.GetCurrent();
        node.Ident    = MakeIdent(name.StringValue);

        var c1        = tokenizer.EatToken();
        if (AssertSymbol(c1, Colon, err)) return null;

        var c2 = tokenizer.EatToken();
        if (AssertSymbol(c2, Colon, err)) return null;

        var next = tokenizer.EatToken();
        if (AssertSymbol(next, ORParen, err)) return null;

        if (tokenizer.Peek().Type != CRParen) {
            node.Args = new List<AstNode>();
            // parse arguments
            while (next.Type != EndOfFile) {
                next = tokenizer.EatToken(); // next is ident

                if (next.Type == Comma) next = tokenizer.EatToken();
                if (next.Type == CRParen) break;
                if (AssertSymbol(next, TokenType.Ident, err)) return null;

                var colon = tokenizer.EatToken();
                if (AssertSymbol(colon, Colon, err)) return null;

                var ident       = MakeIdent(next.StringValue);
                AstNode  assign = null;
                TypeInfo type   = null;

                // @Incomplete suggest type of argument
                if (tokenizer.Peek().Type == TokenType.Equals) {
                    tokenizer.EatToken();
                    tokenizer.EatToken();
                    assign = MakeAssign(ident, ParseExpression(tokenizer, err, -9999));
                    if (assign.Expression.Type == AstType.Literal) {
                        type = assign.Expression.TypeInfo;
                    }
                } else {
                    var t = tokenizer.EatToken();
                    type  = TypeSystem.GetType(t.StringValue);
                }

                var vardecl = MakeVar(ident, type, assign);

                node.Args.Add(vardecl);
            }
        } else {
            tokenizer.EatToken();
        }

        var cur = tokenizer.GetCurrent();
        if (AssertSymbol(cur, CRParen, err)) return null;

        var min = tokenizer.EatToken();

        if (min.Type == OParen) {
            node.TypeInfo = TypeSystem.Void;
        } else {
            if (AssertSymbol(min, Minus, err)) return null;

            var more = tokenizer.EatToken();
            if (AssertSymbol(more, MoreThan, err)) return null;

            var tp = tokenizer.EatToken();
            if (AssertSymbol(tp, TokenType.Ident, err)) return null;

            node.TypeInfo = TypeSystem.GetType(tp.StringValue);
            tokenizer.EatToken();
        }

        node.Body = ParseBody(tokenizer, err);

        return node;
    }

    private static List<AstNode> ParseBody(Tokenizer tokenizer, ErrorStream err) {
        var nodes = new List<AstNode>();

        if (AssertSymbol(tokenizer.GetCurrent(), OParen, err)) return null;

        while (tokenizer.GetCurrent().Type != EndOfFile) {
            var currentToken = tokenizer.EatToken();

            switch (currentToken.Type) {
                case CParen : return nodes;
                case TokenType.Equals :
                    nodes.Add(ParseAssignment(tokenizer, err));
                    break;
                case TokenType.Return : {
                    nodes.Add(ParseReturn(tokenizer, err));
                } break;
                case TokenType.Ident : {
                    var next = tokenizer.Peek();

                    if (next.Type == ORParen) {
                        nodes.Add(ParseFuncall(tokenizer, err));
                        var semicolon = tokenizer.EatToken();

                        if (AssertSymbol(semicolon, Semicolon, err)) return null;
                    }
                    break;
                }
                default : break;
            }
        }

        if (AssertSymbol(tokenizer.GetCurrent(), CParen, err)) return null;

        return nodes;
    }

    private static AstNode ParseFuncall(Tokenizer tokenizer, ErrorStream err) {
        var name      = tokenizer.GetCurrent();
        var node      = new AstNode();
        node.Type     = Statement;
        node.StmtType = Funcall;
        node.Ident    = MakeIdent(name.StringValue);

        tokenizer.EatToken(); // ORParen 100% here
        var cparen = tokenizer.EatToken();

        if (cparen.Type == CRParen) {
            tokenizer.EatToken();

            var semicolon = tokenizer.EatToken();
            if (AssertSymbol(semicolon, Semicolon, err)) return null;

            return node;
        }

        node.Args = new List<AstNode>();

        while (tokenizer.GetCurrent().Type != EndOfFile) {
            var arg  = ParseExpression(tokenizer, err, -9999);
            var next = tokenizer.EatToken();

            node.Args.Add(arg);

            if (next.Type == CRParen) break;
            if (AssertSymbol(next, Comma, err)) return null;

            tokenizer.EatToken();
        }

        cparen = tokenizer.GetCurrent();
        if (AssertSymbol(cparen, CRParen, err)) return null;

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
        node.Type   = AstType.Literal;
        node.String = token.StringValue;

        if(type == null) {
            node.TypeInfo = GuessLiteralType(token.StringValue);
        } else {
            node.TypeInfo = type;
        }

        return node;
    }

    private static AstNode MakeVar(AstNode ident, TypeInfo type, AstNode assign = null) {
        var vardecl      = new AstNode();
        vardecl.Type     = Statement;
        vardecl.StmtType = VarDecl;
        vardecl.Ident    = ident;
        vardecl.TypeInfo = type;
        vardecl.Stmt     = assign;

        return vardecl;
    }

    private static AstNode MakeAssign(AstNode ident, AstNode expr) {
        var assign        = new AstNode();
        assign.Type       = Statement;
        assign.StmtType   = Assign;
        assign.Ident      = ident;
        assign.Expression = expr;

        return assign;
    }

    private static bool IsBinary(Tokenizer tokenizer) {
        var cur  = tokenizer.GetCurrent();

        switch(cur.Type) {
            case Plus    : return true;
            case Minus   : {
                if(tokenizer.Current == 0) return false;
                var prev = tokenizer.Previous();
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

    private static bool AssertSymbol(Token token, TokenType type, ErrorStream err) {
        if (token.Type != type) {
            err.UnexpectedSymbol(token.Line, token.Column, type, token.Type);
            return true;
        }

        return false;
    }
}