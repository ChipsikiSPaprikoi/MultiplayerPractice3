using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);

    private Camera _cam;

    private void Start()
    {
        if (base.Owner != null && base.Owner.IsLocalClient)
        {
            _cam = Camera.main;
            Debug.Log($"{gameObject.name}: Camera enabled for owner");
        }
    }

    private void LateUpdate()
    {
        if (_cam == null || base.Owner == null || !base.Owner.IsLocalClient) return;
    
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
