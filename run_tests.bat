@ECHO OFF

cd tests

call mcs -sdk:4.5 -define:DEBUG -out:tests.exe Tests.cs ../*.cs -main:Tests /unsafe -debug

tests.exe

cd ..



