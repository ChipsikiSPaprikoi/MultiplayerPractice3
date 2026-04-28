using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _lifetime = 5f;

    private float _spawnTime;

    public override void OnNetworkSpawn()
    {
        _spawnTime = Time.time;
        Debug.Log($"[Projectile] Spawned by Owner={OwnerClientId}");
    }

    private void Update()
    {
        if (!IsSpawned) return;
        
        float age = Time.time - _spawnTime;
        if (age > _lifetime)
        {
            NetworkObject.Despawn(destroy: true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !IsSpawned) return;
        
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            NetworkObject.Despawn(destroy: true);
            return;
        }

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;
        
        if (target.OwnerClientId == OwnerClientId) return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

        NetworkObject.Despawn(destroy: true);
    }
}
