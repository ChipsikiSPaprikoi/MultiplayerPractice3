using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Сетевые статы")]
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    [Header("Респавн (координаты для префаба)")]
    [SerializeField] private Vector3[] _spawnPositions = new Vector3[]
    {
        new Vector3(-5f, 1f, 0f),   
        new Vector3(5f, 1f, 0f),    
        new Vector3(0f, 1f, -5f),   
        new Vector3(0f, 1f, 5f)     
    };

    private const float PlayerSpacing = 3f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"{name}: IsLocalPlayer={IsLocalPlayer} | IsOwner={IsOwner} | ClientId={OwnerClientId}");
        
        if (IsLocalPlayer)
        {
            SetNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
        
        Nickname.OnValueChanged += OnNicknameChanged;
        HP.OnValueChanged += OnHPChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;
    }

    public override void OnNetworkDespawn()
    {
        Nickname.OnValueChanged -= OnNicknameChanged;
        HP.OnValueChanged -= OnHPChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
        base.OnNetworkDespawn();
    }

    private void OnHPChanged(int previous, int current)
    {
        Debug.Log($"HP изменен: {previous} -> {current}");
        
        if (!IsServer) return;
        if (current <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log($"{Nickname.Value} мёртв, ждём респавн...");
        yield return new WaitForSeconds(5f);
        
        Vector3 respawnPos;
        if (_spawnPositions.Length > 0)
        {
            int idx = Random.Range(0, _spawnPositions.Length);
            respawnPos = _spawnPositions[idx];
            Debug.Log($"{Nickname.Value} возродился в spawnPos[{idx}] = {respawnPos}");
        }
        else
        {
            respawnPos = new Vector3(OwnerClientId * PlayerSpacing, 1f, 0f);
            Debug.LogWarning("Нет spawnPositions! Fallback по ClientId.");
        }

        transform.position = respawnPos;
        HP.Value = 100;
        IsAlive.Value = true;
        Debug.Log($"{Nickname.Value} возродился! HP={HP.Value}, Pos={respawnPos}");
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        Debug.Log($"{Nickname.Value} IsAlive: {prev} -> {next}");
        GetComponent<MeshRenderer>().enabled = next;
        GetComponent<CharacterController>().enabled = next;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetNicknameServerRpc(string nickname)
    {
        if (Nickname.Value.IsEmpty)
        {
            Vector3 startPos = _spawnPositions.Length > 0 
                ? _spawnPositions[(int)OwnerClientId % _spawnPositions.Length] 
                : new Vector3(OwnerClientId * PlayerSpacing, 1f, 0f);
            transform.position = startPos;
        }
        
        string safeNickname = string.IsNullOrWhiteSpace(nickname) 
            ? $"Player_{OwnerClientId}" 
            : nickname.Trim().Substring(0, Mathf.Min(30, nickname.Length));
        
        Nickname.Value = new FixedString32Bytes(safeNickname);
    }

    private void OnNicknameChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        Debug.Log($"Ник изменен: {previous.ToString()} -> {current.ToString()}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsAlive.Value) return;
        HP.Value = Mathf.Max(0, HP.Value - damage);
    }
}
