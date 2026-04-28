using System.Collections;
using TMPro;
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
        yield return new WaitForSeconds(3f);
        
        PlayerNetwork[] players = FindObjectsOfType<PlayerNetwork>();
        if (players.Length > 0)
        {
            _playerNetwork = players[0];
            Debug.Log($"RespawnTimerUI: Найден игрок {_playerNetwork.name}");
        }
        else
        {
            Debug.LogWarning("RespawnTimerUI: Игроки не найдены!");
        }
    }

    private void Update()
    {
        if (_playerNetwork != null && _playerNetwork.IsAlive.Value)
        {
            HidePanel();
        }
        else if (_playerNetwork != null)
        {
            ShowPanel();
        }
    }

    private void OnDestroy()
    {
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
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
