using System.Collections.Generic;

public static class StackExtensions
{
    public static T[] PopMany<T>(this Stack<T> stack, int count)
    {
        T[] result = new T[count];
        for (int i = 0; i < count; i++) result[i] = stack.Pop();
        return result;
    }

    public static void PushMany<T>(this Stack<T> stack,IEnumerable<T> items)
    {
        foreach (T item in items) stack.Push(item);
    }
}