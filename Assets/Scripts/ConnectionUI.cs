using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private GameObject _uiPanel;

    public static string PlayerNickname { get; private set; } = "Player";

    private void Awake() 
    {
        if (_hostButton != null)
            _hostButton.onClick.AddListener(StartAsHost);
        
        if (_clientButton != null)
            _clientButton.onClick.AddListener(StartAsClient);
    }

    public void StartAsHost()
    {
        SaveNickname();
        NetworkManager.Singleton.StartHost();
        HideUI();
    }

    public void StartAsClient()
    {
        SaveNickname();
        NetworkManager.Singleton.StartClient();
        HideUI();
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void HideUI()
    {
        if (_uiPanel != null)
            _uiPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_hostButton != null)
            _hostButton.onClick.RemoveListener(StartAsHost);
        if (_clientButton != null)
            _clientButton.onClick.RemoveListener(StartAsClient);
    }
}
