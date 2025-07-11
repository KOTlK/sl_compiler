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
        // var path      = $"{directory}/test_file.sl";
        // var text      = File.ReadAllText(path);
        var err       = new ErrorStream();
        // var sb        = new StringBuilder();
        // var tokenizer = Lexer.Tokenize(text);

        // var ast = AstParser.Parse(tokenizer, err);

        // if(err.Count > 0) {
        //     print(err.ToString());
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

        // Text.text = sb.ToString();
        var cu = new CodeUnit(256);
        cu.PushFunction(2, 0, 4, 4, 4);
        cu.Pushlarg(0);
        cu.Pushsloc(0);
        cu.Pushlarg(1);
        cu.Pushsloc(1);
        cu.Pushlloc(0);
        cu.Pushlloc(1);
        cu.Push(add_s32);
        cu.Push(ret);
        cu.PushFunction(2, 0, 4, 4, 4);
        cu.Pushlarg(0);
        cu.Pushlarg(1);
        cu.PushCall(10);
        cu.Push(ret);
        cu.PushMain(0);
        cu.Push(push_s32, 10);
        cu.Push(push_s32, 5);
        cu.PushCall(44);
        cu.Push(ret);

        if (File.Exists($"{directory}/Test.cu")) {
            File.Delete($"{directory}/Test.cu");
        }

        var f = File.Create($"{directory}/Test.cu");
        f.Write(cu.Bytes, 0, cu.Count);
        f.Close();

        SLVM.Init();

        Print(SLVM.BytecodeToString(cu.Bytes, cu.Count));
        var r = SLVM.Run(cu, err);

        if(err.Count > 0) {
            Print(err.ToString());
        }

        Print(r.ToString());
    }

    private static void Print(string str) {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log(str);
#else
        Console.WriteLine(str);
#endif
    }
}