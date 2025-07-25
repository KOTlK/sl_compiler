public enum Opcode : ushort {
    // push 32 bit integer
    push_s32  = 0,
    push_size = 8,
    // pop 32 bit integer
    pop_s32   = 9,
    // pop 2 32 bit integers, add them, push value
    add_s32   = 10,
    // pop 2 32 bit integers, sub them, push value
    sub_s32   = 11,
    // function declaration
    func      = 12,
    // function call
    call      = 13,
    // return
    ret       = 14,
    // push local variable or function argument at specified index on stack
    llocal    = 15,
    // pop from stack and set this value to local variable or function argument at specified index
    slocal    = 16,
}