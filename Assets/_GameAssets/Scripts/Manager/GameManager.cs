using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public event Action<GaemState> OnGameSataeChange;

    [SerializeField] private GameDataSO _gameDataSO;
    [SerializeField] private GaemState _currentGameState;

    private NetworkVariable<int> _gameTimer = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;    
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _gameTimer.Value = _gameDataSO.GameTimer;
            SetTimerTextRpc();
            InvokeRepeating(nameof(DecreaseTimer), 1f, 1f);
        }

        _gameTimer.OnValueChanged += OnTimerChanged;
    }

    private void OnTimerChanged(int previousValue, int newValue)
    {
        TimerUI.Instance.SetTimerUI(newValue);

        if (IsServer && newValue <= 0)
        {
            ChangeGameState(GaemState.GameOver);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetTimerTextRpc()
    {
        TimerUI.Instance.SetTimerUI(_gameTimer.Value);
    }

    private void DecreaseTimer()
    {
        if (IsServer && _currentGameState == GaemState.Playing)
        {
            _gameTimer.Value--;

            if (_gameTimer.Value <= 0)
            {
                CancelInvoke(nameof(DecreaseTimer));
            }
        }
    }

    public void ChangeGameState(GaemState newGaemState)
    {
        if (!IsServer) { return; }

        _currentGameState = newGaemState;
        ChangeGameStateRpc(newGaemState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeGameStateRpc(GaemState newGameState)
    {
        _currentGameState = newGameState;
        OnGameSataeChange?.Invoke(newGameState);
        Debug.Log($"Gaem State: {newGameState}");
    }

    public GaemState GetGameState()
    {
        return _currentGameState;
    }
}
