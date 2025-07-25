using System;
using System.IO;
using System.Text;

using static TokenType;
using static Opcode;
using static Context;

public enum TestResult {
    OK    = 0,
    NOTOK = 1,
}

public static class Tests {
    public delegate TestResult TestFunc();

    public struct Test {
        public string   Name;
        public TestFunc Func;

        public Test(string name, TestFunc func) {
            Name = name;
            Func = func;
        }
    }

    public static Test[] AllTests = new Test[] {
        new Test (nameof(BytecodeFuncall), BytecodeFuncall),
    };

    public static TestResult RunAllTests() {
        var result = TestResult.OK;

        foreach (var test in AllTests) {
            result = test.Func();

            if (result != TestResult.OK) {
                Console.WriteLine($"Test {test.Name} failed");
                return result;
            } else {
                Console.WriteLine($"Test {test.Name} passed");
            }
        }

        if (result == TestResult.OK) {
            Console.WriteLine("All tests passed");
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
        var firstFun = cu.PushFunction(2, 2);
        cu.SetFunctionPos(1, firstFun);
        cu.PushMath(add, 0, 0, 0, 1);
        cu.PushReturn(0);
        var secondFun = cu.PushFunction(2, 2);
        cu.SetFunctionPos(2, secondFun);
        cu.PushCall(1);
        cu.PushReturn(0);
        cu.PushMain(2);
        cu.Pushset_s32(0, 10);
        cu.Pushset_s32(1, 5);
        cu.PushCall(2);
        cu.PushReturn(0);

        if (File.Exists($"{directory}/Test.cu")) {
            File.Delete($"{directory}/Test.cu");
        }

        var f = File.Create($"{directory}/Test.cu");
        f.Write(cu.Bytes, 0, (int)cu.Count);
        f.Close();

        SLVM.Init();

        Console.WriteLine(SLVM.BytecodeToString(cu.Bytes, cu.Count));
        var r = SLVM.Run(cu);

        if (r != 15) {
            Console.WriteLine($"The result is wrong, should be 15, but got {r.ToString()}");
            return TestResult.NOTOK;
        }

        if (SLVM.StackCurrent != 0) {
            Console.WriteLine($"The remain of the stack is wront, should be 0, got {SLVM.StackCurrent.ToString()}");
            return TestResult.NOTOK;
        }

        return TestResult.OK;
    }
}
