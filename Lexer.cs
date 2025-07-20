using System.Collections.Generic;
using System.Text;
using Enum = System.Enum;
using System.Runtime.InteropServices;

using static TokenType;
using static Context;

public struct Token {
    public TokenType Type;
    public string    StringValue;
    public Value     LiteralValue;
    public int       Line;
    public int       Column;
}

[StructLayout(LayoutKind.Explicit)]
public struct Value {
    [FieldOffset(0)]
    public ulong  IntValue;
    [FieldOffset(0)]
    public float  FloatValue;
    [FieldOffset(0)]
    public double DoubleValue;
}

public class Lexer {
    public string      Text;
    public List<Token> Tokens;
    public int         TextPtr;
    public int         TokensPtr;
    public int         Line;
    public int         LineStart;
    public int         Len;

    private StringBuilder sb;

    private static HashSet<string> Keywords = new HashSet<string>() {
        "if",
        "struct",
        "return",
        "for",
        "while",
        "break",
        "continue",
        "switch",
        "case",
        "default",
        "static",
    };

    public Lexer(string text) {
        Text      = text;
        TextPtr   = 0;
        TokensPtr = 0;
        Line      = 1;
        LineStart = 1;
        Tokens    = ListPool<Token>.Get();
        sb        = new StringBuilder();
        Len       = text.Length;
    }

    public void Reset() {
        TextPtr = 0;
        TokensPtr = 0;
        Line = 1;
        LineStart = 1;
    }

    public Token GetCurrent() {
        if (Tokens.Count == 0) {
            return EatToken();
        }

        return Tokens[TokensPtr - 1];
    }

    public Token EatToken() {
        if (TokensPtr < Tokens.Count) {
            var token = Tokens[TokensPtr++];
            return token;
        }

        if (TextPtr == Len - 1) {
            var token    = new Token();
            token.Type   = EndOfFile;
            token.Line   = Line;
            token.Column = Line-LineStart;

            Tokens.Add(token);
            TokensPtr++;
            return token;
        }


        var tkn = ParseToken();
        Tokens.Add(tkn);
        TokensPtr++;

        return tkn;
    }

    public Token Peek(int pos = 1) {
        var token = new Token();
        var index = TokensPtr - 1 + pos;

        if (index >= Tokens.Count) {
            var count = index - Tokens.Count + 1;
            for (var i = 0; i < count; ++i) {
                token = ParseToken();
                Tokens.Add(token);
            }
        } else {
            token = Tokens[index];
        }

        return token;
    }

    public Token Previous(int pos = 1) {
        var index  = TokensPtr - 1 - pos;
        var token  = new Token();
        token.Type = Unknown;

        if (index < 0) {
            return token;
        }

        return Tokens[index];
    }

    private static bool IsNumber(char c) {
        return (c >= '0' && c <= '9');
    }

    private static bool IsSpecialSymbol(char c) {
        switch(c) {
            case ','  : return true;
            case ':'  : return true;
            case ';'  : return true;
            case '{'  : return true;
            case '}'  : return true;
            case '['  : return true;
            case ']'  : return true;
            case '('  : return true;
            case ')'  : return true;
            case '.'  : return true;
            case '*'  : return true;
            case '&'  : return true;
            case '%'  : return true;
            case '^'  : return true;
            case '$'  : return true;
            case '#'  : return true;
            case '@'  : return true;
            case '!'  : return true;
            case '"'  : return true;
            case '\'' : return true;
            case '\\' : return true;
            case '|'  : return true;
            case '/'  : return true;
            case '<'  : return true;
            case '>'  : return true;
            case '-'  : return true;
            case '='  : return true;
            case '+'  : return true;
            case '~'  : return true;
            default   : return false;
        }
    }

    private Token ParseToken() {
        for (; TextPtr < Len; ++TextPtr) {
            var c = Text[TextPtr];

            switch (c) {
                case '\r' : break;
                case '\t' : break;
                case ' '  : break;
                case '-'  : {
                    var token = new Token();

                    if (Text[TextPtr + 1] == '>') {
                        token.Type   = ArrowRight;
                        token.Line   = Line;
                        token.Column = TextPtr - LineStart;
                        TextPtr += 2;
                        return token;
                    }

                    token.Type   = Minus;
                    token.Line   = Line;
                    token.Column = TextPtr - LineStart;
                    TextPtr++;
                    return token;
                }
                case ':'  : {
                    var token = new Token();

                    if (Text[TextPtr + 1] == ':') {
                        token.Type   = DoubleColon;
                        token.Line   = Line;
                        token.Column = TextPtr - LineStart;
                        TextPtr += 2;
                        return token;
                    }

                    token.Type   = Colon;
                    token.Line   = Line;
                    token.Column = TextPtr - LineStart;
                    TextPtr++;
                    return token;
                }
                case '\n' :
                    Line++;
                    LineStart = TextPtr;
                    break;
                case (char)Div : {
                    if (Text[TextPtr+1] == '/') {
                        TextPtr++;
                        for ( ; TextPtr < Len; ++TextPtr) {
                            if (Text[TextPtr] == '\n') {
                                Line++;
                                LineStart = TextPtr;
                                break;
                            }
                        }
                        break;
                    }

                    var token    = new Token();
                    token.Type   = Div;
                    token.Line   = Line;
                    token.Column = TextPtr - LineStart;
                    TextPtr++;
                    return token;
                }
                case (char)SQuote : {
                    var token    = new Token();
                    token.Type   = CharLiteral;
                    token.Line   = Line;
                    token.Column = TextPtr - LineStart;
                    TextPtr++;

                    if (Text[TextPtr] == (char)SQuote) {
                        Err.Push($"Lexer Error {Line}:{TextPtr-LineStart}. Expected character, got {SQuote}. Did you mean \"\'\"?");
                        return new Token();
                    }

                    if (Text[TextPtr] == '\\') {
                        TextPtr++;
                    }

                    token.StringValue = $"{Text[TextPtr]}";

                    TextPtr++;

                    if (Text[TextPtr] != (char)SQuote) {
                        Err.Push($"Lexer Error {Line}:{TextPtr-LineStart}. Expected closing {SQuote}, got {Text[TextPtr]}.");
                        return new Token();
                    }

                    TextPtr++;
                    return token;
                }
                case (char)DQuote : {
                    var token    = new Token();
                    token.Type   = StringLiteral;
                    token.Line   = Line;
                    token.Column = TextPtr-LineStart;
                    sb.Clear();

                    TextPtr++;

                    for ( ; TextPtr < Len; ++TextPtr) {
                        if(Text[TextPtr] == '"' && Text[TextPtr-1] != '\\') break;

                        var shouldAppend = true;

                        if (Text[TextPtr] == '\\' && Text[TextPtr+1] == 'n') {
                            shouldAppend = false;
                            TextPtr++;
                            sb.Append('\n');
                        }
                        if (Text[TextPtr] == '\\' && Text[TextPtr+1] == 't') {
                            shouldAppend = false;
                            TextPtr++;
                            sb.Append('\t');
                        }
                        if (Text[TextPtr] == '\\' && Text[TextPtr+1] == '\"') {
                            shouldAppend = false;
                            TextPtr++;
                            sb.Append('\"');
                        }

                        if (shouldAppend) sb.Append(Text[TextPtr]);
                    }

                    if (Text[TextPtr] != '"') {
                        Err.Push($"Lexer Error {Line}:{TextPtr-LineStart}. Expected closing {DQuote}, but got {Text[TextPtr]}");
                        return new Token();
                    }

                    TextPtr++;
                    return token;
                }
                default : {
                    sb.Clear();
                    var col = TextPtr - LineStart;

                    if (IsSpecialSymbol(Text[TextPtr])) {
                        var token    = new Token();
                        token.Type   = (TokenType)Text[TextPtr];
                        token.Line   = Line;
                        token.Column = col;
                        TextPtr++;
                        return token;
                    } else if (IsNumber(Text[TextPtr])) {
                        for ( ; TextPtr < Len; ++TextPtr) {
                            if (IsSpecialSymbol(Text[TextPtr])) {
                                if(Text[TextPtr] != '.') break;
                            }
                            if (Text[TextPtr] == ' ')             break;
                            if (Text[TextPtr] == '\t')            break;
                            if (Text[TextPtr] == '\r')            break;
                            if (Text[TextPtr] == '\n')            break;

                            sb.Append(Text[TextPtr]);
                        }
                        TextPtr--;

                        var txt = sb.ToString();
                        sb.Clear();

                        var token         = new Token();

                        // parse double/float value
                        if (txt.Contains(".")) {
                            if (txt[txt.Length - 1] == 'f' || txt[txt.Length - 1] == 'F') {
                                var t = txt.Substring(0, txt.Length - 1);
                                if (float.TryParse(t, out var f)) {
                                    token.Type = FloatLiteral;
                                    token.LiteralValue.FloatValue = f;
                                } else {
                                    Err.Push("Unexpected number format at %:%", Line, col);
                                }
                            } else if (txt[txt.Length - 1] == 'd' || txt[txt.Length - 1] == 'D') {
                                var t = txt.Substring(0, txt.Length - 1);
                                if (double.TryParse(t, out var d)) {
                                    token.Type                     = DoubleLiteral;
                                    token.LiteralValue.DoubleValue = d;
                                } else {
                                    Err.Push("Unexpected number format at %:%", Line, col);
                                }
                            } else {
                                if (double.TryParse(txt, out var d)) {
                                    token.Type                     = DoubleLiteral;
                                    token.LiteralValue.DoubleValue = d;
                                } else {
                                    Err.Push("Unexpected number format at %:%", Line, col);
                                }
                            }
                        } else {
                            if (ulong.TryParse(txt, out var i)) {
                                token.Type = IntLiteral;
                                token.LiteralValue.IntValue = i;
                            } else {
                                Err.Push("Unexpected number format at %:%", Line, col);
                                return new Token();
                            }
                        }

                        token.Line        = Line;
                        token.Column      = col;
                        TextPtr++;
                        return token;
                    } else {
                        for ( ; TextPtr < Len; ++TextPtr) {
                            if (IsSpecialSymbol(Text[TextPtr]))   break;
                            if (Text[TextPtr] == ' ')             break;
                            if (Text[TextPtr] == '\t')            break;
                            if (Text[TextPtr] == '\r')            break;
                            if (Text[TextPtr] == '\n')            break;

                            sb.Append(Text[TextPtr]);
                        }
                        TextPtr--;

                        var txt = sb.ToString();
                        sb.Clear();

                        if (Keywords.Contains(txt)) {
                            var token         = new Token();
                            if (Enum.TryParse(txt, true, out TokenType keyword)) {
                                token.Type        = keyword;
                                token.StringValue = txt;
                                token.Line        = Line;
                                token.Column      = col;
                                TextPtr++;
                                return token;
                            } else {
                                Err.Push($"Cannot parse keyword %", txt);
                            }
                        }
                        // thank you microsoft.
                        var tkn2         = new Token();
                        tkn2.Type        = Ident;
                        tkn2.StringValue = txt;
                        tkn2.Line        = Line;
                        tkn2.Column      = col;
                        TextPtr++;
                        return tkn2;
                    }
                }
            }
        }

        // thank you microsoft.
        var tkn = new Token();

        tkn.Type = Unknown;

        return tkn;
    }
}