public enum RegType : byte {
    s32    = 0,
    u32    = 1,
    s64    = 2,
    u64    = 3,
    Float  = 4,
    Double = 5,
}

public enum Opcode : ushort {
    // usage | description
    // set regType dest 10          |   set constant into register

    set      ,
    // mov dest src                 |   move src register to dest
    mov      ,
    // add regType dest a b         |   add a and b and store result to dest
    add      ,
    sub      ,
    mul      ,
    div      ,
    mod      ,
    // .func regCount argCount      |   function declaration.
    func     ,
    // call funIndex                |   call function
    call     ,
    // ret reg                      |   return
    ret      ,
    // cmp regType r0 r1            |   compare values in registers and store the result in
    //                                  FLAGS register
    cmp      ,
    // jmp label                    |   jump to label in a bytecode
    jmp      ,
    // jl label                     |   jump if less
    jl       ,
    // jg label                     |   jump if greater
    jg       ,
    // je label                     |   jump if equals
    je       ,
    // jne label                    |   jump if not equals
    jne      ,
    // jz label                     |   jump if zero
    jz       ,
    // jnz label                    |   jump if not zero
    jnz      ,
}