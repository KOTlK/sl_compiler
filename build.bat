@echo off

SET src=Main.cs Assertions.cs Ast.cs AstNode.cs CodeUnit.cs Context.cs DictionaryPool.cs ErrorStream.cs Lexer.cs ListExt.cs ListPool.cs Opcode.cs TokenType.cs TypeSystem.cs BytecodeConstants.cs BytecodeConverter.cs SLVM.cs Register.cs

call mcs -sdk:4.5 -define:DEBUG -out:main.exe -main:Program %src% /unsafe -debug