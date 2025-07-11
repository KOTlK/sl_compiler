public enum Opcode : ushort {
    // push 32 bit integer
    push_s32  = 0,
    // pop 32 bit integer
    pop_s32   = 1,
    // pop 2 32 bit integers, add them, push value
    add_s32   = 2,
    // pop 2 32 bit integers, sub them, push value
    sub_s32   = 3,
    // function declaration
    func      = 4,
    // function call
    call      = 5,
    // return
    ret       = 6,
    // push argument at specified index on stack
    larg      = 7,
    // push local variable at specified index on stack
    lloc      = 8,
    // pop from stack and set this value to local variable at specified index
    sloc      = 9,
}