@echo off

SET src=Interpreter.cs Assertions.cs Context.cs DictionaryPool.cs ErrorStream.cs ListExt.cs ListPool.cs Opcode.cs BytecodeConstants.cs SLVM.cs Register.cs

call mcs -sdk:4.5 -define:DEBUG -out:sli.exe %src% -main:Interpreter /unsafe -debug