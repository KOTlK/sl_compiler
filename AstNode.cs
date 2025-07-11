using System.Text;
using System.Collections.Generic;

public enum AstType {
    None          = 0,
    Ident         = 1,
    Statement     = 2,
    Expression    = 3,
    Literal       = 4,
    Operator      = 5,
}

public enum StatementType {
    VarDecl  = 0,
    Assign   = 1,
    Return   = 2,
    Typedef  = 3,
    Fundef   = 4,
    Funcall  = 5,
}

public class AstNode {
    public AstType       Type;
    public string        String;
    public TokenType     OperatorType;
    public StatementType StmtType;
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
            case AstType.Literal : {
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
            case AstType.Statement : {
                sb.Append(' ', indent * spaces);
                sb.Append($"{StmtType}: ");
                sb.Append('\n');
                indent++;
                if(StmtType == StatementType.VarDecl) {
                    if (TypeInfo == null) {
                        Ident.Draw(sb, ref indent);
                    } else {
                        sb.Append(' ', indent * spaces);
                        sb.Append($"{Ident.String}: {TypeInfo.Name}");
                    }
                    if (Stmt != null) {
                        Stmt.Draw(sb, ref indent);
                    }
                } else if (StmtType == StatementType.Assign) {
                    Ident.Draw(sb, ref indent);
                    if (Literal != null) {
                        Literal.Draw(sb, ref indent);
                    } else if (Expression != null) {
                        Expression.Draw(sb, ref indent);
                    }
                } else if (StmtType == StatementType.Return) {
                    Expression.Draw(sb, ref indent);
                } else if (StmtType == StatementType.Typedef) {
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
                } else if (StmtType == StatementType.Fundef) {
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
                } else if (StmtType == StatementType.Funcall) {
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
                }
                indent--;
                sb.Append('\n');
            } break;
        }
    }
}