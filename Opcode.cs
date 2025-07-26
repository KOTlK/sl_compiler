public enum Opcode : ushort {
    // usage | description
    // set_s32 dest 10              |   push 32 bit integer
    set_s32  ,
    set_u32  ,
    set_s64  ,
    set_u64  ,
    fset     ,
    fset64   ,
    // mov dest src                 |   move src register to dest
    mov      ,
    // add dest a b                 |   add 2 integers and put the result to dest
    // add regType dest a b         |   regType: 0 - 32-bit signed   integer
    //                                           1 - 32-bit unsigned integer
    //                                           2 - 64-bit signed   integer
    //                                           3 - 64-bit unsigned integer
    add      ,
    // fadd regType dest a b        |   add 2 floats and put the result to dest
    //                                  regType: 0 - 32-bit float
    //                                           1 - 64-bit float
    sub      ,
    mul      ,
    div      ,
    mod      ,
    fadd     ,
    fsub     ,
    fmul     ,
    fdiv     ,
    fmod     , // (C)
    // .func regCount argCount      |   function declaration.
    func     ,
    // call funIndex                |   call function
    call     ,
    // ret reg                      |   return
    ret      ,
}