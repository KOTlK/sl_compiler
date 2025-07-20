using System;
using System.IO;
using System.Text;

using static TokenType;
using static AstParser;
using static Opcode;

public enum TestResult {
    OK    = 0,
    NOTOK = 1,
}

public static class Tests {
    public static TestResult RunAllTests() {
        var err = new ErrorStream();
        Context.Init(err);
        var result = TestResult.OK;

        if (BytecodeFuncall() != TestResult.OK) {
            Console.WriteLine($"Test {nameof(BytecodeFuncall)} failed");
            return TestResult.NOTOK;
        } else {
            Console.WriteLine($"Test {nameof(BytecodeFuncall)} passed");
        }

        return result;
    }

    public static TestResult BytecodeFuncall() {
 #if UNITY_EDITOR || UNITY_STANDALONE
        var directory = $"{Application.streamingAssetsPath}";
#else
        var directory  = $"{AppDomain.CurrentDomain.BaseDirectory}";
#endif
        var cu  = new CodeUnit(256);
        cu.Push(3);
        cu.Push(0);
        cu.Push(0);
        cu.Push(1);
        cu.Push(0);
        cu.Push(2);
        cu.Push(0);
        var firstFun = cu.PushFunction(2, 2, 4, 4, 4, 4, 4);
        cu.SetFunctionPos(1, firstFun);
        cu.Pushlarg(0);
        cu.Pushslocal(0);
        cu.Pushlarg(1);
        cu.Pushslocal(1);
        cu.Pushllocal(0);
        cu.Pushllocal(1);
        cu.Push(add_s32);
        cu.Push(ret);
        var secondFun = cu.PushFunction(2, 0, 4, 4, 4);
        cu.SetFunctionPos(2, secondFun);
        cu.Pushlarg(0);
        cu.Pushlarg(1);
        cu.PushCall(1);
        cu.Push(ret);
        cu.PushMain(0);
        cu.Push(push_s32, 10);
        cu.Push(push_s32, 5);
        cu.PushCall(2);
        cu.Push(ret);

        if (File.Exists($"{directory}/Test.cu")) {
            File.Delete($"{directory}/Test.cu");
        }

        var f = File.Create($"{directory}/Test.cu");
        f.Write(cu.Bytes, 0, (int)cu.Count);
        f.Close();

        SLVM.Init();

        Console.WriteLine(SLVM.BytecodeToString(cu.Bytes, cu.Count));
        var r = SLVM.Run(cu);

        if (r != 15)                return TestResult.NOTOK;
        if (SLVM.StackCurrent != 0) return TestResult.NOTOK;

        return TestResult.OK;
    }
}
