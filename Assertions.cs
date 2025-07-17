using System.Diagnostics;
using System;

public static class Assertions {
    [Conditional("DEBUG")]
    public static void Assert(bool expr) {
        if(!expr) {
            Console.WriteLine("Assertion Failed");
        }
    }

    [Conditional("DEBUG")]
    public static void Assert(bool expr, string errorMessage) {
        if(!expr) {
            Console.WriteLine(errorMessage);
        }
    }
}