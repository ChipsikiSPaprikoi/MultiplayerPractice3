using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Managing;
using UnityEngine;
using FishNet.CodeGenerating;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] public int _maxAmmo = 10;

    [AllowMutableSyncType] public SyncVar<int> CurrentAmmo = new SyncVar<int>(10);

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        
        if (base.IsServerInitialized)
            CurrentAmmo.Value = _maxAmmo;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    private void Update()
    {
        if (!base.Owner.IsLocalClient || !_playerNetwork.IsAlive.Value) return;
    
        if (Input.GetKeyDown(KeyCode.Space))
            ShootServerRpc(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, NetworkConnection senderConnection = null)
    {
        if (!_playerNetwork.IsAlive.Value || _playerNetwork.HP.Value <= 0) return;
        if (CurrentAmmo.Value <= 0) return;
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        CurrentAmmo.Value--;

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        ServerManager.Spawn(go, senderConnection);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(dir * 40f, ForceMode.Impulse);
        
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.SetOwner(base.Owner);
    }
}
