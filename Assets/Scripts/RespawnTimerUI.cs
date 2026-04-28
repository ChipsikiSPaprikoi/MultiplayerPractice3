using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RespawnTimerUI : MonoBehaviour
{
    [SerializeField] private GameObject _respawnPanel;
    [SerializeField] private TMP_Text _timerText;

    private PlayerNetwork _playerNetwork;
    private Coroutine _timerCoroutine;

    private void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    private IEnumerator WaitForLocalPlayer()
    {
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        _playerNetwork = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();

        if (_playerNetwork != null)
        {
            _playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;
            OnIsAliveChanged(true, _playerNetwork.IsAlive.Value);
        }
    }

    private void OnDestroy()
    {
        if (_playerNetwork != null)
            _playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnIsAliveChanged(bool previous, bool current)
    {
        if (current)
            HidePanel();
        else
            ShowPanel();
    }

    private void ShowPanel()
    {
        if (_respawnPanel != null)
            _respawnPanel.SetActive(true);

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void HidePanel()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        if (_timerText != null)
            _timerText.text = string.Empty;

        if (_respawnPanel != null)
            _respawnPanel.SetActive(false);
    }

    private IEnumerator TimerRoutine()
    {
        float timeLeft = 5f;

        while (timeLeft > 0f)
        {
            if (_timerText != null)
                _timerText.text = $"Respawn: {timeLeft:F1}s";

            timeLeft -= Time.deltaTime;
            yield return null;
        }
    }
}
