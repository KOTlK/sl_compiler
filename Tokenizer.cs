using System.Collections.Generic;
using System.Text;

using static TokenType;

public class Tokenizer {
    public List<Token> Tokens;
    public int         Current;
    public int         Length;

    public Token GetCurrent() {
        return Tokens[Current];
    }

    public Token EatToken() {
        Current++;

        return Tokens[Current];
    }

    public Token Previous(int i = 1) {
        return Tokens[Current - i];
    }

    public Token Peek(int i = 1) {
        return Tokens[Current + i];
    }

    public override string ToString() {
        var sb = new StringBuilder();

        for(var i = 0; i < Length; ++i) {
            var token = Tokens[i];
            sb.Append($"{token.Type}, ");
            if(token.StringValue != null) {
                sb.Append($"{token.StringValue}, ");
            }
            sb.Append($"{token.Line}:{token.Column}");
            sb.Append('\n');
        }

        return sb.ToString();
    }
}