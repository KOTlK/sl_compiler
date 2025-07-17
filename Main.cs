using System;
using System.IO;
using System.Text;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using TMPro;
#endif

using static TokenType;
using static AstParser;
using static Opcode;
using static Tests;

#if UNITY_EDITOR || UNITY_STANDALONE
public class Main : MonoBehaviour {
    private void Start() {
#else
public static class Program {
    public static void Main(string[] args) {
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        var directory = $"{Application.streamingAssetsPath}";
#else
        var directory  = $"{AppDomain.CurrentDomain.BaseDirectory}";
#endif
        var path      = $"{directory}/test_file.sl";
        // var text      = File.ReadAllText(path);
        var err       = new ErrorStream();
        // var sb        = new StringBuilder();
        // var lexer     = new Lexer(text, err);

        // var ast = AstParser.Parse(lexer, err);

        // var token = lexer.EatToken();

        // while (token.Type != TokenType.EndOfFile) {
        //     if (token.Type == 0)       break;
        //     if (token.Type == Unknown) break;

        //     sb.Append($"{token.Type}, ");
        //     if (token.Type == IntLiteral) {
        //         sb.Append($"{token.LiteralValue.IntValue} ");
        //     } else if (token.Type == FloatLiteral) {
        //         sb.Append($"{token.LiteralValue.FloatValue} ");
        //     } else if (token.Type == DoubleLiteral) {
        //         sb.Append($"{token.LiteralValue.DoubleValue} ");
        //     } else if (token.Type == StringLiteral) {
        //         sb.Append($"{token.StringValue} ");
        //     } else if (token.Type == Ident) {
        //         sb.Append($"{token.StringValue} ");
        //     }

        //     sb.Append($"{token.Line}:{token.Column}");
        //     sb.Append('\n');

        //     token = lexer.EatToken();
        // }

        // if(err.Count > 0) {
        //     Print(err.ToString());
        // }

        // foreach(var node in ast.Typedefs) {
        //     var indent = 0;
        //     node.Draw(sb, ref indent);
        //     sb.Append('\n', 1);
        // }

        // sb.Append('\n', 3);

        // foreach(var node in ast.Functions) {
        //     var indent = 0;
        //     node.Draw(sb, ref indent);
        //     sb.Append('\n', 1);
        // }

        // sb.Append('\n', 3);

        // foreach(var node in ast.Nodes) {
        //     var indent = 0;
        //     node.Draw(sb, ref indent);
        //     sb.Append('\n', 3);
        // }

        // Print(sb.ToString());

        Tests.RunAllTests();
    }

    private static void Print(string str) {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log(str);
#else
        Console.WriteLine(str);
#endif
    }
}