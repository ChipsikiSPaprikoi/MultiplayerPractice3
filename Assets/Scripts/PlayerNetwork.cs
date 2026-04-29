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
        Debug.Log($"{name}: Network spawned (Owner={base.Owner?.ClientId}, IsServer={base.IsServerInitialized})");
    }

    private void Start()
    {
        StartCoroutine(DelayedNicknameRpc());
    }

    private IEnumerator DelayedNicknameRpc()
    {
        yield return new WaitForSeconds(0.1f);

        if (base.IsServerInitialized)
        {
            ConnectionUI conn = FindObjectOfType<ConnectionUI>();
            string nick = conn != null ? conn.PlayerNickname : null;

            string safeNickname = string.IsNullOrWhiteSpace(nick)
                ? $"Player_{base.Owner?.ClientId ?? 0}"
                : nick.Trim().Substring(0, Mathf.Min(30, nick.Length));

            Debug.Log($"[Server] Setting nickname for {name}: '{safeNickname}'");
            SetNicknameServerRpc(safeNickname);
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[Client] {name}, Owner={base.Owner?.ClientId}, IsLocalClient={base.Owner?.IsLocalClient}");
    }

    private void OnHPChanged(int previous, int current)
    {
        Debug.Log($"HP: {previous} → {current}");

        if (base.IsServerInitialized && current <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(5f);

        Vector3 respawnPos;
        if (_spawnPositions.Length > 0)
        {
            int idx = Random.Range(0, _spawnPositions.Length);
            respawnPos = _spawnPositions[idx];
        }
        else
        {
            respawnPos = new Vector3(base.Owner.ClientId * PlayerSpacing, 1f, 0f);
            Debug.LogWarning("Нет spawnPositions! Fallback по ClientId.");
        }

        transform.position = respawnPos;
        HP.Value = 100;
        IsAlive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNicknameServerRpc(string nickname)
    {
        Debug.Log($"[ServerRpc] SetNicknameServerRpc: nickname='{nickname}' invoked for {name} (Owner={base.Owner?.ClientId})");

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
        Debug.Log($"[Server] Nickname set to '{Nickname.Value}' for {name}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsAlive.Value) return;

        HP.Value = Mathf.Max(0, HP.Value - damage);

        if (HP.Value <= 0)
        {
            Die();
        }
    }

    private void OnIsAliveChanged(bool prev, bool next, bool asServer)
    {
        Debug.Log($"[IsAlive] {Nickname.Value} IsAlive: {prev} → {next}, AsServer={asServer}");
        
    }

    private void Die()
    {
        if (!IsAlive.Value)
            return;

        Debug.Log($"[PlayerNetwork] {Nickname.Value} умирает, IsAlive = false");
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        IsAlive.Value = false;
        UpdateVisualObserversRpc(false);
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Respawn();
    }

    private void Respawn()
    {
        transform.position = _spawnPositions[Random.Range(0, _spawnPositions.Length)];
        HP.Value = 100;
        IsAlive.Value = true;
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = true;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = true;
        
        UpdateVisualObserversRpc(true);

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.CurrentAmmo.Value = shooting._maxAmmo;
        }
    }
    
    [ObserversRpc]
    public void UpdateVisualObserversRpc(bool isAlive)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        CharacterController controller = GetComponent<CharacterController>();

        if (renderer != null)
            renderer.enabled = isAlive;

        if (controller != null)
            controller.enabled = isAlive;
    }
}
