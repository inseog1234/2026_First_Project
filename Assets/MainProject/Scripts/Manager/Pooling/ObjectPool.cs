using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> available = new();
    private readonly T prefab;
    private readonly Transform root;

    // 선택 Get/Return 시 액션
    private readonly System.Action<T> onGet;
    private readonly System.Action<T> onReturn;

    public int CountAvailable => available.Count;

    public ObjectPool(T prefab, int preload, Transform parent, string poolName = null,
        System.Action<T> onGet = null, System.Action<T> onReturn = null)
    {
        this.prefab = prefab;
        this.onGet = onGet;
        this.onReturn = onReturn;

        root = new GameObject(string.IsNullOrEmpty(poolName) ? $"{prefab.name}_Pool" : poolName).transform;
        root.SetParent(parent, false);

        for (int i = 0; i < preload; i++)
            available.Enqueue(Create());
    }

    private T Create()
    {
        T obj = Object.Instantiate(prefab, root);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        T obj = (available.Count > 0) ? available.Dequeue() : Create();
        obj.gameObject.SetActive(true);
        onGet?.Invoke(obj);
        return obj;
    }

    public void Return(T obj)
    {
        onReturn?.Invoke(obj);
        obj.transform.SetParent(root, false);
        obj.gameObject.SetActive(false);
        available.Enqueue(obj);
    }
}
