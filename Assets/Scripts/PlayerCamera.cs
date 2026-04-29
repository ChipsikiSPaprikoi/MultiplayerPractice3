using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        bool isLocal = base.Owner != null && base.Owner.IsLocalClient;
    
        if (!isLocal || _cam == null) return;
    
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position);
    }
}
