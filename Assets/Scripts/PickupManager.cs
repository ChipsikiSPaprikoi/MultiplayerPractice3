using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections;

public class PickupManager : NetworkBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    private bool _initialized = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _initialized = true;
        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (var point in _spawnPoints)
        {
            SpawnPickup(point.position);
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        var pickup = go.GetComponent<HealthPickup>();
        if (pickup != null) pickup.Init(this);
        ServerManager.Spawn(go);
    }
}
