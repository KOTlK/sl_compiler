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
        new Test (nameof(BytecodeIf), BytecodeIf),
    };

    private static string TestsDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}";

    public static void Main() {
        var err       = new ErrorStream();
        Context.Init(err);

        var ok = RunAllTests();

        if (ok != TestResult.OK) {
            Console.WriteLine(err.ToString());
            return;
        }
    }

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
        var cu  = new CodeUnit(256, 3, 0);
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

        if (File.Exists($"{TestsDirectory}/{nameof(BytecodeFuncall)}.cu")) {
            File.Delete($"{TestsDirectory}/{nameof(BytecodeFuncall)}.cu");
        }

        var f = File.Create($"{TestsDirectory}/{nameof(BytecodeFuncall)}.cu");
        f.Write(cu.Bytes, 0, (int)cu.Count);
        f.Close();

        SLVM.Init();

        Console.WriteLine(SLVM.BytecodeToString(cu.Bytes, cu.Count));
        var r = SLVM.Run(cu.Bytes);

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

    public static TestResult BytecodeIf() {
        var result = TestResult.OK;
        var cu     = new CodeUnit(256, 1, 1);
        var main = cu.PushMain(2);
        cu.SetFunctionPos(0, main);
        cu.Pushset_s32(0, 10);
        cu.Pushset_s32(1, 11);
        cu.PushCmp(RegType.s32, 0, 1);
        cu.PushJumpIf(jl, 0);
        cu.PushReturn(0);
        var labelPos = cu.Count;
        cu.SetLabelPos(0, labelPos);
        cu.PushReturn(1);

        if (File.Exists($"{TestsDirectory}/{nameof(BytecodeIf)}.cu")) {
            File.Delete($"{TestsDirectory}/{nameof(BytecodeIf)}.cu");
        }

        var f = File.Create($"{TestsDirectory}/{nameof(BytecodeIf)}.cu");
        f.Write(cu.Bytes, 0, (int)cu.Count);
        f.Close();

        SLVM.Init();

        Console.WriteLine(SLVM.BytecodeToString(cu.Bytes, cu.Count));
        var r = SLVM.Run(cu.Bytes);

        if (r != 11) {
            result = TestResult.NOTOK;
            return result;
        }

        return result;
    }
}
