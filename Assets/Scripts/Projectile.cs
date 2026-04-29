using FishNet.Object;
using UnityEngine;
using FishNet.Connection;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _lifetime = 5f;

    private float _spawnTime;
    private NetworkConnection _owner;

    public void SetOwner(NetworkConnection owner)
    {
        _owner = owner;
    }

    public override void OnStartNetwork()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (!base.IsSpawned) return;
        
        float age = Time.time - _spawnTime;
        if (age > _lifetime)
        {
            ServerManager.Despawn(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized) return;
        
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            ServerManager.Despawn(gameObject);
            return;
        }

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null || !target.IsAlive.Value) return;
        
        if (_owner == target.Owner)
            return;
        
        target.TakeDamageServerRpc(_damage);
    
        ServerManager.Despawn(gameObject);
    }
}
