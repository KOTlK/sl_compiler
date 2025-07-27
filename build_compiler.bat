@echo off

SET src=Compiler.cs Assertions.cs Ast.cs AstNode.cs CodeUnit.cs Context.cs DictionaryPool.cs ErrorStream.cs Lexer.cs ListExt.cs ListPool.cs Opcode.cs TokenType.cs TypeSystem.cs BytecodeConstants.cs BytecodeConverter.cs

call mcs -sdk:4.5 -define:DEBUG -out:slc.exe %src% -main:Compiler /unsafe -debug