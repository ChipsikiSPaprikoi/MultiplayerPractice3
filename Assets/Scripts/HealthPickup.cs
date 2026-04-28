using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;

    public void Init(PickupManager manager)
    {
        _manager = manager;
        _spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized) return;

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;
        
        if (!player.IsAlive.Value) return;
        
        if (player.HP.Value >= 100) return;
        
        player.HP.Value = Mathf.Min(100, player.HP.Value + _healAmount);
        Debug.Log($"{player.Nickname} подобрал аптечку: HP {player.HP.Value - _healAmount} → {player.HP.Value}");
        
        _manager.OnPickedUp(_spawnPosition);
        ServerManager.Despawn(gameObject);
    }
}
