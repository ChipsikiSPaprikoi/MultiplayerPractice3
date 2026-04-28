using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private PlayerShooting _playerShooting;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _ammoText;

    public override void OnStartNetwork()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
        if (_playerShooting == null)
            _playerShooting = GetComponent<PlayerShooting>();
        
        UpdateAllUI();
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    private void UpdateAllUI()
    {
        if (_nicknameText != null)
            _nicknameText.text = _playerNetwork.Nickname.Value.ToString();
    
        if (_hpText != null)
            _hpText.text = $"HP: {_playerNetwork.HP.Value}";
    
        if (_ammoText != null && _playerShooting != null)
            _ammoText.text = $"Ammo: {_playerShooting.CurrentAmmo.Value}/{_playerShooting._maxAmmo}";
    }

    private void Update()
    {
        UpdateAllUI();
    }
}
