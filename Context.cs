public static class Context {
    public static ErrorStream Err;

    public static void Init(ErrorStream err) {
        Err = err;
    }
}