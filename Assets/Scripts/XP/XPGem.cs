using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XPGem : MonoBehaviour
{
    public int xpValue;
    public float magnetRange = 3f;
    public float moveSpeed = 10f;

    private Transform _playerTransform;
    private XPManager _xpManager;

    public void Initialize(Transform playerTransform, XPManager xpManager)
    {
        _playerTransform = playerTransform;
        _xpManager = xpManager;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist < magnetRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            _xpManager.CollectGem(gameObject, xpValue);
        }
    }
}
