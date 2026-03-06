using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleReturnToPool : MonoBehaviour
{
    private ObjectPool _pool;
    private GameObject _target;

    public void Setup(ObjectPool pool, GameObject target)
    {
        _pool = pool;
        _target = target;
    }

    private void OnParticleSystemStopped()
    {
        _pool?.Return(_target);
    }
}
