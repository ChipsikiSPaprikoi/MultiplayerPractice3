using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private PlayerNetwork _playerNetwork;
    private float _verticalVelocity;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!base.Owner.IsLocalClient || _playerNetwork == null || !_playerNetwork.IsAlive.Value) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        MoveServerRpc(h, v);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(float h, float v)
    {
        if (_playerNetwork == null || !_playerNetwork.IsAlive.Value) return;
        if (!_cc.enabled) return;

        Vector3 move = new Vector3(h, 0f, v).normalized * _speed;

        _verticalVelocity += _gravity * Time.deltaTime;
        if (_cc.isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;

        move.y = _verticalVelocity;
        _cc.Move(move * Time.deltaTime);
    }
}
