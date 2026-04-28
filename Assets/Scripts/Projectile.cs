using FishNet.Object;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _lifetime = 5f;

    private float _spawnTime;

    public override void OnStartNetwork()
    {
        _spawnTime = Time.time;
        Debug.Log("[Projectile] Spawned");
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
        if (!base.IsServerInitialized || !base.IsSpawned) return;
        
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            ServerManager.Despawn(gameObject);
            return;
        }

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;
        
        if (Vector3.Distance(transform.position, target.transform.position) < 2f) return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;
        target.TakeDamageServerRpc(_damage);

        ServerManager.Despawn(gameObject);
    }
}
