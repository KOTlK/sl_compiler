using System.Collections.Generic;

using static Assertions;

public static class ListExt {
    public static T Last<T>(this List<T> list) {
        if (list.Count > 0) return list[list.Count - 1];
        return default(T);
    }
}