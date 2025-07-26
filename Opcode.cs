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
    // add dest a b                 |   add 2 32-bit integers and put the result into dest
    add_s32  ,
    add_u32  ,
    add_s64  ,
    add_u64  ,
    fadd     ,
    fadd64   ,
    sub_s32  ,
    mul_s32  ,
    div_s32  ,
    mod_s32  ,
    // .func regCount argCount      |   function declaration.
    func     ,
    // call funIndex                |   call function
    call     ,
    // ret reg                      |   return
    ret      ,
}