using System.Collections.Generic;

public static class ListPool<T> {
    public static Stack<List<T>> Stack = new Stack<List<T>>();

    public static List<T> Get() {
        if (Stack.Count > 0) return Stack.Pop();

        return new List<T>();
    }

    public static void Release(List<T> l) {
        l.Clear();
        Stack.Push(l);
    }
}