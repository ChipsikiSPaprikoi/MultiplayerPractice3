using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class PlayerView : NetworkBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private PlayerShooting _playerShooting;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _ammoText;

    public override void OnNetworkSpawn()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
        if (_playerShooting == null)
            _playerShooting = GetComponent<PlayerShooting>();
        
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged += OnHpChanged;
        if (_playerShooting != null)
            _playerShooting.CurrentAmmo.OnValueChanged += OnAmmoChanged;
        
        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);
        OnAmmoChanged(0, _playerShooting?.CurrentAmmo.Value ?? 10);
    }

    public override void OnNetworkDespawn()
    {
        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
            _playerNetwork.HP.OnValueChanged -= OnHpChanged;
        }
        if (_playerShooting != null)
            _playerShooting.CurrentAmmo.OnValueChanged -= OnAmmoChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (_nicknameText != null)
            _nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newValue}";
    }

    private void OnAmmoChanged(int oldValue, int newValue)
    {
        if (_ammoText != null)
            _ammoText.text = $"Ammo: {newValue}/{_playerShooting?._maxAmmo ?? 10}";
    }
}
