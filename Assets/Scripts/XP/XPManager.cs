using UnityEngine;

public class XPManager : MonoBehaviour
{
    public int[] xpThresholds = { 10, 25, 50, 100, 200 };
    public GameObject xpGemPrefab;
    public float gemMagnetRange = 2f;
    public float gemMoveSpeed = 10f;

    private ObjectPool _gemPool;
    private XPSystem _xpSystem;
    private Transform _playerTransform;

    public XPSystem XPSystem => _xpSystem;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _xpSystem = new XPSystem(xpThresholds);

        var poolParent = new GameObject("XPGemPool").transform;
        poolParent.SetParent(transform);
        _gemPool = new ObjectPool(xpGemPrefab, 30, poolParent);
    }

    public void DropGem(Vector3 position, int xpValue)
    {
        GameObject gem = _gemPool.Get();
        gem.transform.position = position;
        var gemComponent = gem.GetComponent<XPGem>();
        if (gemComponent != null)
        {
            gemComponent.xpValue = xpValue;
            gemComponent.Initialize(_playerTransform, this);
        }
    }

    public void CollectGem(GameObject gem, int xpValue)
    {
        _xpSystem.AddXP(xpValue);
        _gemPool.Return(gem);
    }
}
