public static class BytecodeConstants {
    public const uint FunctionsCountOffset = 4;
    public const uint FunctionsOffset      = 8;
    public const uint MainPos              = 12;
    public const int  StackSize            = 1024 * 1024 * 8; // 8mb stack
    public const uint FrameHeader          = 14;
    public const uint ArgsCountOffset      = 1;
    public const uint LocalsCountOffset    = 2;
    public const uint RetSizeOffset        = 6;
    public const uint PrevFpOffset         = 10;
    public const uint OldPcOffset          = 14;
    public const uint ArgOffsetSize        = 2;
    public const uint LocalOffsetSize      = 2;
}