using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class StartingGameUI : NetworkBehaviour
{
    public static StartingGameUI Instance { get; private set; }

    public event Action OnAllPlayersCounnected;

    [Header("Reference")]
    [SerializeField] private TMP_Text _countdownText;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _waitingSeconds = 1;

    private NetworkVariable<int> _playersLoaded = new NetworkVariable<int>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        Instance = this;         
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SetPlayerLoaderRpc();
        }

        if (IsServer)
        {
            OnSinglePlayerConnected();
            _playersLoaded.OnValueChanged += OnPlayerLoadedChanged;
        }
    }

    private void OnPlayerLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (IsServer && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartCountdownRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerLoaderRpc()
    {
        _playersLoaded.Value++;
        Debug.Log("Client Scene loaded. Total Loaded: " + _playersLoaded.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartCountdownRpc()
    {
        OnAllPlayersCounnected?.Invoke();
        StartCoroutine(CountdownCourtine());
    }

    private void OnSinglePlayerConnected()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            StartCoroutine(CountdownCourtine());
            WaitingForPlayersUI.Instance.Hide();
        }
    }

    private IEnumerator CountdownCourtine()
    {
        _countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            AnimateText();
            yield return new WaitForSeconds(_waitingSeconds);
        }

        GameManager.Instance.ChangeGameState(GaemState.Playing);
        _countdownText.text = "GO!";
        AnimateText();
        yield return new WaitForSeconds(_waitingSeconds);

        _countdownText.transform.DOScale(0f, _animationDuration / 2).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            _countdownText.gameObject.SetActive(false);
        });
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.zero;
        _countdownText.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30, 30));

        _countdownText.transform.DOScale(1f, _animationDuration).SetEase(Ease.OutBack);
        _countdownText.transform.DORotate(Vector3.zero, _animationDuration).SetEase(Ease.OutBack);
    }
}
