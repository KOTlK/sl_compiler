using System;
using System.IO;
using System.Text;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using TMPro;
#endif

// using static TokenType;
// using static AstParser;
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
        var text      = File.ReadAllText(path);
        var err       = new ErrorStream();
        Context.Init(err);
        var sb        = new StringBuilder();
        // var ast       = AstParser.Parse(lexer);
        // SLVM.Init();

        var ok = Tests.RunAllTests();

        if (ok != TestResult.OK) {
            Print(err.ToString());
            return;
        }

        if(err.Count > 0) {
            Print(err.ToString());
            return;
        }

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

        // var cu = BytecodeConverter.AstToBytecode(ast);

        // if(err.Count > 0) {
        //     Print(err.ToString());
        //     return;
        // }

        // var bytecode = SLVM.BytecodeToString(cu.Bytes, cu.Count);

        // if(err.Count > 0) {
        //     Print(err.ToString());
        //     return;
        // }

        // Print(bytecode);

        // var run = SLVM.Run(cu);

        // if(err.Count > 0) {
        //     Print(err.ToString());
        //     return;
        // }

        // Print(run.ToString());
        // Print(SLVM.StackCurrent.ToString());
    }

    private static void Print(string str) {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log(str);
#else
        Console.WriteLine(str);
#endif
    }
}