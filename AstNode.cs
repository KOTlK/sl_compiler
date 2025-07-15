using System.Text;
using System.Collections.Generic;

public enum AstType {
    None             = 0,
    Ident            = 1,
    StatementVarDecl = 3,
    StatementAssign  = 4,
    StatementReturn  = 5,
    StatementTypedef = 6,
    StatementFundef  = 7,
    StatementFuncall = 8,
    Expression       = 9,
    StringLiteral    = 10,
    CharLiteral      = 11,
    IntLiteral       = 12,
    FloatLiteral     = 13,
    DoubleLiteral    = 14,
    Operator         = 15,
}

public class AstNode {
    public AstType       Type;
    public string        String;
    public Value         Number;
    public TokenType     OperatorType;
    public bool          IsBinary;
    public AstNode       Ident;
    public AstNode       Literal;
    public AstNode       Expression;
    public AstNode       Stmt;
    public AstNode       Operator;
    public AstNode       Left;
    public AstNode       Right;
    public TypeInfo      TypeInfo;
    public List<AstNode> Args;
    public List<AstNode> Body;

    public void Draw(StringBuilder sb, ref int indent) {
        const int spaces = 3;
        switch(Type) {
            case AstType.Ident : {
                sb.Append(' ', indent * spaces);
                sb.Append(String);
                sb.Append('\n');
            } break;
            case AstType.IntLiteral : {
                sb.Append(' ', indent * spaces);
                sb.Append(TypeInfo.Name);
                sb.Append(" : ");
                sb.Append(Number.IntValue);
                sb.Append('\n');
            } break;
            case AstType.FloatLiteral : {
                sb.Append(' ', indent * spaces);
                sb.Append(TypeInfo.Name);
                sb.Append(" : ");
                sb.Append(Number.FloatValue);
                sb.Append('\n');
            } break;
            case AstType.DoubleLiteral : {
                sb.Append(' ', indent * spaces);
                sb.Append(TypeInfo.Name);
                sb.Append(" : ");
                sb.Append(Number.DoubleValue);
                sb.Append('\n');
            } break;
            case AstType.CharLiteral :
            case AstType.StringLiteral : {
                sb.Append(' ', indent * spaces);
                sb.Append(TypeInfo.Name);
                sb.Append(" : ");
                sb.Append(String);
                sb.Append('\n');
            } break;
            case AstType.Operator : {
                sb.Append(' ', indent * spaces);
                if(IsBinary) {
                    sb.Append($"{OperatorType}:");
                    sb.Append('\n');
                    indent++;
                    Left.Draw(sb, ref indent);
                    Right.Draw(sb, ref indent);
                    indent--;
                } else {
                    sb.Append($"Unary{OperatorType}:");
                    sb.Append('\n');
                    indent++;
                    Right.Draw(sb, ref indent);
                    indent--;
                }
                sb.Append('\n');
            } break;
            case AstType.StatementVarDecl : {
                sb.Append(' ', indent * spaces);
                sb.Append("VarDecl: ");
                sb.Append('\n');
                indent++;
                if (TypeInfo == null) {
                    Ident.Draw(sb, ref indent);
                } else {
                    sb.Append(' ', indent * spaces);
                    sb.Append($"{Ident.String}: {TypeInfo.Name}");
                }
                if (Stmt != null) {
                    Stmt.Draw(sb, ref indent);
                }
                indent--;
                sb.Append('\n');
            } break;
            case AstType.StatementAssign : {
                sb.Append(' ', indent * spaces);
                sb.Append("Assign: ");
                sb.Append('\n');
                indent++;
                Ident.Draw(sb, ref indent);
                if (Literal != null) {
                    Literal.Draw(sb, ref indent);
                } else if (Expression != null) {
                    Expression.Draw(sb, ref indent);
                }
                indent--;
                sb.Append('\n');
            } break;
            case AstType.StatementReturn : {
                sb.Append(' ', indent * spaces);
                sb.Append("Return: ");
                sb.Append('\n');
                indent++;
                Expression.Draw(sb, ref indent);
                indent--;
                sb.Append('\n');
            } break;
            case AstType.StatementTypedef : {
                sb.Append(' ', indent * spaces);
                sb.Append("Typedef: ");
                sb.Append('\n');
                indent++;
                sb.Append(TypeInfo.Name);
                sb.Append(":\n");
                indent++;
                foreach(var field in TypeInfo.Fields) {
                    sb.Append(' ', spaces * indent);
                    sb.Append($"{field.Name} : {field.Type.Name}\n");
                }
                sb.Append(' ', spaces * indent);
                sb.Append($"Align: {TypeInfo.Align}\n");
                sb.Append(' ', spaces * indent);
                sb.Append($"Size:  {TypeInfo.Size}\n");
                indent--;
                indent--;
                sb.Append('\n');
            } break;
            case AstType.StatementFundef : {
                sb.Append(' ', indent * spaces);
                sb.Append("Fundef: ");
                sb.Append('\n');
                indent++;
                Ident.Draw(sb, ref indent);
                if (TypeInfo != null) {
                    sb.Append(' ', spaces * indent);
                    sb.Append($"Return type: {TypeInfo.Name}\n");
                }
                sb.Append(' ', spaces * indent);
                sb.Append("Args:\n");
                indent++;
                if (Args != null) {
                    foreach(var arg in Args) {
                        arg.Draw(sb, ref indent);
                    }
                }
                indent--;
                sb.Append(' ', spaces * indent);
                sb.Append("Body:\n");
                indent++;
                foreach(var node in Body) {
                    node.Draw(sb, ref indent);
                }
                indent--;
                indent--;
                sb.Append('\n');
            } break;
            case AstType.StatementFuncall : {
                sb.Append(' ', indent * spaces);
                sb.Append("Call: ");
                sb.Append('\n');
                indent++;
                Ident.Draw(sb, ref indent);
                sb.Append(' ', spaces * indent);
                sb.Append("Args:\n");
                if (Args != null) {
                    indent++;
                    foreach(var arg in Args) {
                        arg.Draw(sb, ref indent);
                    }
                    indent--;
                }

                if (TypeInfo != null) {
                    sb.Append(' ', spaces * indent);
                    sb.Append($"Return type: {TypeInfo.Name}");
                }
                indent--;
                sb.Append('\n');
            } break;
        }
    }
}