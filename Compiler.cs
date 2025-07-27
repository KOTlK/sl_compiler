using System.IO;
using System.Collections.Generic;
using System.Text;

public struct CompiledSource {
    public string   Name;
    public string   Path;
    public CodeUnit Bytecode;
}

public struct SourceFile {
    public string Text;
    public string Path;
    public string Name;
}

public static class Compiler {
    public static bool SaveBytecode = true;
#if UNITY_EDITOR || UNITY_STANDALONE
#else
    public static void Main(string[] args) {
        if (args.Length == 0) {
            Print("Arguments array is empty");
        }
        var config = new List<string>();
        var source = new List<SourceFile>();

        foreach(var arg in args) {
            if (arg.StartsWith("-")) {
                config.Add(arg);
            } else {
                if (arg.EndsWith(".sl") == false) {
                    Print("Source file name should end with .sl");
                    return;
                }
                var text = File.ReadAllText(arg);
                var name = arg.Substring(0, arg.Length - 3);

                source.Add(new SourceFile {
                    Text = text,
                    Path = arg,
                    Name = name
                });
            }
        }

        ProcessConfig(config);
        Compile(source);
    }
#endif

    public static void Compile(List<SourceFile> source) {
        var compiled = new List<CompiledSource>();

        foreach(var file in source) {
            var err       = new ErrorStream();
            Context.Init(err);
            var lexer     = new Lexer(file.Text);
            var ast       = AstParser.Parse(lexer);

            if(err.Count > 0) {
                Print(err.ToString());
                return;
            }

            var cu = BytecodeConverter.AstToBytecode(ast);

            if(err.Count > 0) {
                Print(err.ToString());
                return;
            }

            compiled.Add(new CompiledSource {
                Name     = file.Name,
                Path     = file.Path,
                Bytecode = cu,
            });
        }

        if (SaveBytecode) {
            var sb = new StringBuilder();
            foreach(var code in compiled) {
                for (var i = 0; i < code.Path.Length; ++i) {
                    if (code.Path[i] == '.') break;

                    sb.Append(code.Path[i]);
                }

                sb.Append(".cu");
                var name = sb.ToString();
                sb.Clear();

                if (File.Exists(name)) {
                    File.Delete(name);
                }

                var f = File.Create(name);
                f.Write(code.Bytecode.Bytes, 0, (int)code.Bytecode.Count);
                f.Close();
            }
        }
    }

    public static void ProcessConfig(List<string> configs) {
    }

    private static void Print(string str) {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log(str);
#else
        System.Console.WriteLine(str);
#endif
    }
}