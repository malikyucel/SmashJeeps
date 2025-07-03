using System;
using DG.Tweening;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CanvasGroup[] _canvasGroups;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration;

    private void Start()
    {
        GameManager.Instance.OnGameSataeChange += GameManager_OnGameStateChange;
    }

    private void GameManager_OnGameStateChange(GaemState gaemState)
    {
        if (gaemState == GaemState.GameOver)
        {
            CloseOtherUI();
        }
    }

    private void CloseOtherUI()
    {
        foreach (CanvasGroup canvasGroup in _canvasGroups)
        {
            canvasGroup.DOFade(0f, _fadeDuration);
        }
    }
}
