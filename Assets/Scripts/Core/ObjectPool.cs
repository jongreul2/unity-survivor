using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _available = new();

    public ObjectPool(GameObject prefab, int initialSize, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _available.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _parent);
        }
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _available.Enqueue(obj);
    }
}
