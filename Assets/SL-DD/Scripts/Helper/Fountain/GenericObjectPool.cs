using System;
using System.Collections.Generic;
using UnityEngine;

internal sealed class GenericObjectPool<T> where T : Component
{
    private readonly Func<T> createItem;
    private readonly Queue<T> available = new Queue<T>();
    private readonly HashSet<T> inUse = new HashSet<T>();

    internal GenericObjectPool(Func<T> createItem, int initialCount)
    {
        this.createItem = createItem;
        for (int i = 0; i < initialCount; i++)
            available.Enqueue(CreateInactive());
    }

    internal T Rent()
    {
        T item = available.Count > 0 ? available.Dequeue() : CreateInactive();
        inUse.Add(item);
        item.gameObject.SetActive(true);
        return item;
    }

    internal void Return(T item)
    {
        if (inUse.Remove(item))
            Recycle(item);
    }

    internal void ReturnAll()
    {
        foreach (T item in inUse)
            Recycle(item);

        inUse.Clear();
    }

    private T CreateInactive()
    {
        T item = createItem();
        item.gameObject.SetActive(false);
        return item;
    }

    private void Recycle(T item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(false);
        available.Enqueue(item);
    }
}
