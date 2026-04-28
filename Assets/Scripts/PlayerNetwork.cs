using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using FishNet.CodeGenerating;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Сетевые статы")]
    [AllowMutableSyncType] public SyncVar<string> Nickname = new SyncVar<string>("Player");
    [AllowMutableSyncType] public SyncVar<int> HP = new SyncVar<int>(100);
    [AllowMutableSyncType] public SyncVar<bool> IsAlive = new SyncVar<bool>(true);

    [Header("Респавн (координаты для префаба)")]
    [SerializeField] private Vector3[] _spawnPositions = new Vector3[]
    {
        new Vector3(-5f, 1f, 0f),   
        new Vector3(5f, 1f, 0f),    
        new Vector3(0f, 1f, -5f),   
        new Vector3(0f, 1f, 5f)     
    };

    private const float PlayerSpacing = 3f;

    public override void OnStartNetwork()
    {
        Debug.Log($"{name}: Network spawned");
        
        if (base.Owner != null && base.Owner.IsLocalClient)
        {
            SetNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    private void OnHPChanged(int previous, int current)
    {
        Debug.Log($"HP изменен: {previous} -> {current}");
        
        if (!base.IsServerInitialized) return;
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
            respawnPos = new Vector3(base.Owner.ClientId * PlayerSpacing, 1f, 0f);
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
        if (string.IsNullOrEmpty(Nickname.Value))
        {
            Vector3 startPos = _spawnPositions.Length > 0 
                ? _spawnPositions[(int)(base.Owner?.ClientId ?? 0) % _spawnPositions.Length] 
                : new Vector3((base.Owner?.ClientId ?? 0) * PlayerSpacing, 1f, 0f);
            transform.position = startPos;
        }
        
        string safeNickname = string.IsNullOrWhiteSpace(nickname) 
            ? $"Player_{base.Owner?.ClientId ?? 0}" 
            : nickname.Trim().Substring(0, Mathf.Min(30, nickname.Length));
        
        Nickname.Value = safeNickname;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsAlive.Value) return;
        HP.Value = Mathf.Max(0, HP.Value - damage);
        Debug.Log($"{Nickname.Value} получил {damage} урона, HP = {HP.Value}");
    }
}
