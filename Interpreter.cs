using System;
using System.IO;

public static class Interpreter {
#if UNITY_EDITOR || UNITY_STANDALONE
#else
    public static void Main(string[] args) {
        if (args.Length == 0) {
            Print("Specify the bytecode file");
        }

        var path = args[0];

        if (path.EndsWith(".cu") == false) {
            Print("Bytecode file should have \".cu\" extension.");
            return;
        }

        var bytes = File.ReadAllBytes(path);

        Run(bytes);
    }
#endif

    public static void Run(byte[] bytes) {
        var err = new ErrorStream();
        Context.Init(err);
        SLVM.Init();
        var run = SLVM.Run(bytes);

        if(err.Count > 0) {
            Print(err.ToString());
            return;
        }

        Print(run.ToString());
    }

    private static void Print(string str) {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log(str);
#else
        Console.WriteLine(str);
#endif
    }
}