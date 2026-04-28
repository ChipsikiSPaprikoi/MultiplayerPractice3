using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] public int _maxAmmo = 10;

    private float _lastShotTime;
    public NetworkVariable<int> CurrentAmmo = new(10,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private PlayerNetwork _playerNetwork;

    public override void OnNetworkSpawn()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        _playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;
        
        if (IsServer)
            CurrentAmmo.Value = _maxAmmo;
    }

    public override void OnNetworkDespawn()
    {
        if (_playerNetwork != null)
            _playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (next && IsServer)
            CurrentAmmo.Value = _maxAmmo;
    }

    private void Update()
    {
        if (!IsLocalPlayer || !_playerNetwork.IsAlive.Value) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
            ShootServerRpc(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpcParams = default)
    {
        if (!_playerNetwork.IsAlive.Value || _playerNetwork.HP.Value <= 0) return;
        
        if (CurrentAmmo.Value <= 0) return;
        
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        CurrentAmmo.Value--;

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(dir * 25f, ForceMode.Impulse);
    }
}
