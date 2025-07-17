using System.Collections.Generic;

public static class DictionaryPool<T, U> {
    public static Stack<Dictionary<T, U>> Stack = new Stack<Dictionary<T, U>>();

    public static Dictionary<T, U> Get() {
        if (Stack.Count > 0) return Stack.Pop();

        return new Dictionary<T, U>();
    }

    public static void Release(Dictionary<T, U> l) {
        l.Clear();
        Stack.Push(l);
    }
}