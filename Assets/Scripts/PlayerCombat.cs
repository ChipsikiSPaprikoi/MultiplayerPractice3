using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [Header("Боевая система")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 4f;
    [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;

    public override void OnNetworkSpawn()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner || !_playerNetwork.IsAlive.Value) return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!IsOwner || _playerNetwork == null || !_playerNetwork.IsAlive.Value) return;
        
        PlayerNetwork target = GetNearestEnemy();
        if (target != null && target != _playerNetwork)
        {
            DealDamageServerRpc(target.NetworkObjectId, _damage);
        }
    }

    private PlayerNetwork GetNearestEnemy()
    {
        PlayerNetwork nearest = null;
        float nearestDistance = _attackRange;
        
        foreach (var kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            if (kvp.Value.TryGetComponent<PlayerNetwork>(out var player))
            {
                if (!player.IsAlive.Value) continue;
                
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (player != _playerNetwork && distance <= nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }
            }
        }
        return nearest;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(ulong targetObjectId, int damage, ServerRpcParams rpcParams = default)
    {
        if (!_playerNetwork.IsAlive.Value) return;
        
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;

        if (targetObject.TryGetComponent<PlayerNetwork>(out var targetPlayer))
        {
            if (targetPlayer == _playerNetwork) return;
            if (!targetPlayer.IsAlive.Value) return;

            int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
            targetPlayer.HP.Value = nextHp;
            
            Debug.Log($"{OwnerClientId} нанес {damage} урона {targetPlayer.Nickname.Value} (HP: {nextHp})");
        }
    }
}
