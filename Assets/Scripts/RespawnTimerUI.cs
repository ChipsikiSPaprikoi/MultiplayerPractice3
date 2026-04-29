using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class RespawnTimerUI : MonoBehaviour
{
    [SerializeField] private GameObject _respawnPanel;
    [SerializeField] private TMP_Text _timerText;

    private PlayerNetwork _localPlayer;
    private Coroutine _timerCoroutine;

    private void Start()
    {
        StartCoroutine(FindLocalPlayer());
    }

    private IEnumerator FindLocalPlayer()
    {
        yield return new WaitForEndOfFrame();

        while (_localPlayer == null)
        {
            PlayerNetwork[] players = FindObjectsOfType<PlayerNetwork>();
            foreach (var p in players)
            {
                if (p.Owner != null && p.Owner.IsLocalClient)
                {
                    _localPlayer = p;
                    _localPlayer.IsAlive.OnChange += OnIsAliveChanged;
                    Debug.Log($"RespawnTimerUI: привязан к {_localPlayer.name} (IsAlive={_localPlayer.IsAlive.Value})");
                    break;
                }
            }

            if (_localPlayer == null)
            {
                yield return null;
            }
        }
    }

    private void OnIsAliveChanged(bool prev, bool next, bool asServer)
    {
        Debug.Log($"RespawnTimerUI: OnIsAliveChanged {prev} → {next}, AsServer={asServer}, для {_localPlayer?.Nickname.Value}");

        if (next)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    private void OnDestroy()
    {
        if (_localPlayer != null)
        {
            _localPlayer.IsAlive.OnChange -= OnIsAliveChanged;
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private void ShowPanel()
    {
        Debug.Log("RespawnTimerUI: ShowPanel вызван");
        if (_respawnPanel == null) return;

        _respawnPanel.SetActive(true);

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void HidePanel()
    {
        Debug.Log("RespawnTimerUI: HidePanel вызван");
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
        Debug.Log("RespawnTimerUI: запущен TimerRoutine");
        float timeLeft = 5f;

        while (timeLeft > 0f)
        {
            if (_timerText != null)
                _timerText.text = $"Respawn: {timeLeft:F1}s";

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("RespawnTimerUI: TimerRoutine завершён");
    }
}
