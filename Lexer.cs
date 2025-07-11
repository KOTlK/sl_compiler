using System.Collections.Generic;
using System.Text;
using Enum = System.Enum;

using static TokenType;

public struct Token {
    public TokenType Type;
    public string    StringValue;
    public int       Line;
    public int       Column;
}

public static class Lexer {
    public static HashSet<string> Keywords = new HashSet<string>() {
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

    public static Tokenizer Tokenize(string text, ErrorStream err) {
        var tokenizer = new Tokenizer();
        var tokens    = new List<Token>();
        var sb        = new StringBuilder();
        var len       = text.Length;
        var line      = 1;
        var lineStart = -1;
        sb = new StringBuilder();

        for(var i = 0; i < len; ++i) {
            var c = text[i];

            switch(c) {
                case '\r' : break;
                case '\t' : break;
                case ' '  : break;
                case '\n' :
                    line++;
                    lineStart = i;
                    break;
                case (char)Div : {
                    if (text[i+1] == '/') {
                        i++;
                        for ( ; i < len; ++i) {
                            if (text[i] == '\n') {
                                line++;
                                lineStart = i;
                                break;
                            }
                        }
                        break;
                    }

                    var token    = new Token();
                    token.Type   = Div;
                    token.Line   = line;
                    token.Column = i - lineStart;
                    tokens.Add(token);
                } break;
                case (char)SQuote : {
                    var token    = new Token();
                    token.Type   = CharLiteral;
                    token.Line   = line;
                    token.Column = i - lineStart;
                    i++;

                    if (text[i] == (char)SQuote) {
                        err.Push($"Lexer Error {line}:{i-lineStart}. Expected character, got {SQuote}. Did you mean \"\'\"?");
                        return null;
                    }

                    if (text[i] == '\\') {
                        i++;
                    }

                    token.StringValue = $"{text[i]}";

                    i++;

                    if (text[i] != (char)SQuote) {
                        err.Push($"Lexer Error {line}:{i-lineStart}. Expected closing {SQuote}, got {text[i]}.");
                        return tokenizer;
                    }

                    tokens.Add(token);
                } break;
                case (char)DQuote : {
                    var token    = new Token();
                    token.Type   = StringLiteral;
                    token.Line   = line;
                    token.Column = i-lineStart;
                    sb.Clear();

                    i++;

                    for ( ; i < len; ++i) {
                        if(text[i] == '"' && text[i-1] != '\\') break;

                        var shouldAppend = true;

                        if (text[i] == '\\' && text[i+1] == 'n') {
                            shouldAppend = false;
                            i++;
                            sb.Append('\n');
                        }
                        if (text[i] == '\\' && text[i+1] == 't') {
                            shouldAppend = false;
                            i++;
                            sb.Append('\t');
                        }
                        if (text[i] == '\\' && text[i+1] == '\"') {
                            shouldAppend = false;
                            i++;
                            sb.Append('\"');
                        }

                        if (shouldAppend) sb.Append(text[i]);
                    }

                    if (text[i] != '"') {
                        err.Push($"Lexer Error {line}:{i-lineStart}. Expected closing {DQuote}, but got {text[i]}");
                        return tokenizer;
                    }

                    tokens.Add(token);
                } break;
                default : {
                    sb.Clear();
                    var col = i - lineStart;

                    if (IsSpecialSymbol(text[i])) {
                        var token    = new Token();
                        token.Type   = (TokenType)text[i];
                        token.Line   = line;
                        token.Column = col;
                        tokens.Add(token);
                    } else if (IsNumber(text[i])) {
                        for ( ; i < len; ++i) {
                            if (IsSpecialSymbol(text[i])) {
                                if(text[i] != '.') break;
                            }
                            if (text[i] == ' ')             break;
                            if (text[i] == '\t')            break;
                            if (text[i] == '\r')            break;
                            if (text[i] == '\n')            break;

                            sb.Append(text[i]);
                        }
                        i--;

                        var txt = sb.ToString();
                        sb.Clear();

                        var token         = new Token();
                        token.Type        = Literal;
                        token.StringValue = txt;
                        token.Line        = line;
                        token.Column      = col;
                        tokens.Add(token);
                    } else {
                        for ( ; i < len; ++i) {
                            if (IsSpecialSymbol(text[i]))   break;
                            if (text[i] == ' ')             break;
                            if (text[i] == '\t')            break;
                            if (text[i] == '\r')            break;
                            if (text[i] == '\n')            break;

                            sb.Append(text[i]);
                        }
                        i--;

                        var txt = sb.ToString();
                        sb.Clear();

                        if (Keywords.Contains(txt)) {
                            var token         = new Token();
                            if (Enum.TryParse(txt, true, out TokenType keyword)) {
                                token.Type        = keyword;
                                token.StringValue = txt;
                                token.Line        = line;
                                token.Column      = col;
                                tokens.Add(token);
                            } else {
                                err.Push($"Cannot parse keyword {txt}");
                            }
                        } else {
                            var token         = new Token();
                            token.Type        = Ident;
                            token.StringValue = txt;
                            token.Line        = line;
                            token.Column      = col;
                            tokens.Add(token);
                        }
                    }
                } break;
            }
        }

        var tkn = new Token();

        tkn.Type   = EndOfFile;
        tkn.Line   = line;
        tkn.Column = 1;

        tokens.Add(tkn);

        tokenizer.Tokens  = tokens;
        tokenizer.Current = 0;
        tokenizer.Length  = tokens.Count;

        return tokenizer;
    }

    public static bool IsNumber(char c) {
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
}