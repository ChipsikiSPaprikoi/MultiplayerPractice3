using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [Header("Боевая система")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 4f;
    [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;

    public override void OnStartNetwork()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!base.Owner.IsLocalClient || !_playerNetwork.IsAlive.Value) return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        PlayerNetwork target = GetNearestEnemy();
        if (target != null && target != _playerNetwork && target.IsAlive.Value)
        {
            target.TakeDamageServerRpc(_damage);
        }
    }

    private PlayerNetwork GetNearestEnemy()
    {
        PlayerNetwork nearest = null;
        float nearestDistance = _attackRange;
        
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
        foreach (var behaviour in behaviours)
        {
            if (behaviour is PlayerNetwork player)
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
    private void DealDamageServerRpc(PlayerNetwork targetPlayer, int damage, NetworkConnection senderConnection = null)
    {
        if (!_playerNetwork.IsAlive.Value) return;

        if (targetPlayer == null) return;

        if (targetPlayer == _playerNetwork) return;
        if (!targetPlayer.IsAlive.Value) return;

        int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = nextHp;
    }
}
