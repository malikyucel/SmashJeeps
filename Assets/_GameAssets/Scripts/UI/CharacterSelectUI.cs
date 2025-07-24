using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Button _mainManuButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _copyButton;
    [SerializeField] private TMP_Text _readyText;
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private Image _copiedImage;

    [Header("Sprites")]
    [SerializeField] private Sprite _greenButtonSprite;
    [SerializeField] private Sprite _redButtonSprite;
    [SerializeField] private Sprite _tickSprite;

    private bool _isPlayerReady;
    private void Awake()
    {
        _mainManuButton.onClick.AddListener(OnMainManuButtonClicked);
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
        _startButton.onClick.AddListener(OnStartButtonClicked);
        _copyButton.onClick.AddListener(OnCopyButtonClicked);
    }

    private void Start()
    {
        _startButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        SetStartButtonInteractable(false);

        CharacterSelectReady.Instance.OnAllPlayersReady += CharacterSelectReady_OnAllPlayersReady;
        CharacterSelectReady.Instance.OnUnreadyChanged += CharacterSelectReady_OnUnreadyChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetwrokListChange += MultiplayerGameManager_OnPlayerDataNetwrokListChange;
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCodeText.text = HostSingleton.Instance.HostGameManager.GetJoinCode();
        }
        else
        {
            _joinCodeText.text = ClientSinglleton.Instance.ClientGameManager.GetJoinCode();
        }
    }

    private void CharacterSelectReady_OnAllPlayersReady()
    {
        SetStartButtonInteractable(true);
    }

    private void CharacterSelectReady_OnUnreadyChanged()
    {
        SetStartButtonInteractable(false);
    }

    private void MultiplayerGameManager_OnPlayerDataNetwrokListChange()
    {
        if (CharacterSelectReady.Instance.AreAllPlayersReady())
        {
            SetStartButtonInteractable(true);
        }
        else
        {
            SetStartButtonInteractable(false);
        }
    }

    private void OnCopyButtonClicked()
    {
        _copiedImage.sprite = _tickSprite;
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void OnMainManuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.ShutDown();
        }

        ClientSinglleton.Instance.ClientGameManager.Disconnect();
    }

    private void OnReadyButtonClicked()
    {
        _isPlayerReady = !_isPlayerReady;

        if (_isPlayerReady)
        {
            SetPlayerReady();
        }
        else
        {
            SetPlayerUneady();
        }
    }

    private void OnStartButtonClicked()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(Const.SceneName.GAME_SCENE, LoadSceneMode.Single);
    }

    private void SetPlayerReady()
    {
        CharacterSelectReady.Instance.SetPlayerReady();
        _readyText.text = "Ready";
        _readyButton.image.sprite = _greenButtonSprite;
    }

    private void SetPlayerUneady()
    {
        CharacterSelectReady.Instance.SetPlayerUnready();
        _readyText.text = "Not ready";
        _readyButton.image.sprite = _redButtonSprite;
    }

    private void SetStartButtonInteractable(bool isActive)
    {
        if (_startButton != null)
        {
            _startButton.interactable = isActive;
        }
    }

    private void OnDestroy()
    {
        CharacterSelectReady.Instance.OnAllPlayersReady -= CharacterSelectReady_OnAllPlayersReady;
        CharacterSelectReady.Instance.OnUnreadyChanged -= CharacterSelectReady_OnUnreadyChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetwrokListChange -= MultiplayerGameManager_OnPlayerDataNetwrokListChange;        
    }
}
