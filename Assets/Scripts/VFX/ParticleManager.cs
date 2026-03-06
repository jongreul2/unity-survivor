using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [System.Serializable]
    public class ParticleEntry
    {
        public ParticleType type;
        public GameObject prefab;
        public int poolSize;
    }

    public ParticleEntry[] entries;

    private readonly Dictionary<ParticleType, ObjectPool> _pools = new();

    private void Awake()
    {
        foreach (var entry in entries)
        {
            var parent = new GameObject($"ParticlePool_{entry.type}").transform;
            parent.SetParent(transform);
            _pools[entry.type] = new ObjectPool(entry.prefab, entry.poolSize, parent);
        }
    }

    public void Play(ParticleType type, Vector3 position, Color color, float scale = 1f)
    {
        if (!_pools.ContainsKey(type)) return;

        GameObject obj = _pools[type].Get();
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * scale;

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            ps.Play();

            var returnToPool = obj.GetComponent<ParticleReturnToPool>();
            if (returnToPool == null)
                returnToPool = obj.AddComponent<ParticleReturnToPool>();
            returnToPool.Setup(_pools[type], obj);
        }
    }
}
