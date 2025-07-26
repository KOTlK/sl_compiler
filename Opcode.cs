public enum Opcode : ushort {
    // usage | description
    // set_s32 r0, 10               |   push 32 bit integer
    set_s32  = 0,
    // push_size = 8,
    // pop_s32 r0                   |   pop 32 bit integer
    // pop_s32   = 9,
    // add r0, r1, r2               |   pop 2 32 bit integers, add them, push value
    add       = 10,
    // sub_s32   = 11,
    // .func regCount               |   function declaration.
    func      = 12,
    // call funIndex, argReg        |   call function
    call      = 13,
    // ret r0                       |   return
    ret       = 14,
    // push local variable or function argument at specified index on stack
    // llocal    = 15,
    // // pop from stack and set this value to local variable or function argument at specified index
    // slocal    = 16,
}