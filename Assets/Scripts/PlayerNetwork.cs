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
    

    private IEnumerator DelayedNicknameRpc()
    {
        yield return new WaitForSeconds(0.1f);

        if (!base.IsOwner)
            yield break;

        ConnectionUI conn = FindObjectOfType<ConnectionUI>();
        string nick = conn != null ? conn.PlayerNickname : null;

        SetNicknameServerRpc(nick);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[Client] {name}, Owner={base.Owner?.ClientId}, IsLocalClient={base.Owner?.IsLocalClient}");

        if (base.Owner != null && base.Owner.IsLocalClient)
            StartCoroutine(DelayedNicknameRpc());
    }

    private void OnHPChanged(int previous, int current)
    {
        Debug.Log($"HP: {previous} → {current}");
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNicknameServerRpc(string nickname)
    {
        Debug.Log($"[ServerRpc] SetNicknameServerRpc: nickname='{nickname}' invoked for {name} (Owner={base.Owner?.ClientId})");

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
        UpdateVisualObserversRpc(true);
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = true;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = true;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.CurrentAmmo.Value = shooting._maxAmmo;
        }
    }
    
    [ObserversRpc(BufferLast = true)]
    public void UpdateVisualObserversRpc(bool isAlive)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = isAlive;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = isAlive;
    }
}
