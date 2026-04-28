using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private float _verticalVelocity;

    private void Awake() => _cc = GetComponent<CharacterController>();

    private void Update()
    {
        if (!base.Owner.IsLocalClient || !GetComponent<PlayerNetwork>().IsAlive.Value) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        MoveServerRpc(h, v);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(float h, float v)
    {
        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork == null) return;
        if (!playerNetwork.IsAlive.Value) return;
        if (!_cc.enabled) return;

        Vector3 move = new Vector3(h, 0f, v).normalized * _speed;

        _verticalVelocity += _gravity * Time.deltaTime;
        if (_cc.isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;

        move.y = _verticalVelocity;
        _cc.Move(move * Time.deltaTime);
    }
}
