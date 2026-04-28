using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);

    private Camera _cam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        _cam = Camera.main;
        Debug.Log($"{gameObject.name}: Camera enabled for owner {OwnerClientId}");
    }

    private void LateUpdate()
    {
        if (_cam == null) return;
        
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
