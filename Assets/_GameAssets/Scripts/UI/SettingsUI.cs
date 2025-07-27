using System;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Referene")]
    [SerializeField] private Button _settingsButton;

    [Header("Settings Menu")]
    [SerializeField] private RectTransform _settingsMenuTransform;
    [SerializeField] private Image _backBackgroundImage;
    [SerializeField] private Button _vsyncButton;
    [SerializeField] private GameObject _vsyncTick;
    [SerializeField] private Button _leaveGameButton;
    [SerializeField] private Button _keepPlayingButton;
    [SerializeField] private Button _copyCodeButton;
    [SerializeField] private Image _copiedImage;
    [SerializeField] private TMP_Text _joinCodeText;

    [Header("Sprites")]
    [SerializeField] private Sprite _tickSprite;
    [SerializeField] private Sprite _crossSprite;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private bool _isAnimating;
    private bool _isVsyncActive;
    private bool _isCopiedJoinCode;

    private void Awake()
    {
        _settingsButton.onClick.AddListener(OnSettingsButtonCliced);
        _vsyncButton.onClick.AddListener(OnVsyncButtonClicked);
        _leaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked); 
        _keepPlayingButton.onClick.AddListener(OnKeepPlayingButtonClicked);
        _copyCodeButton.onClick.AddListener(OnCopyCodeButtonCliced);
    }

    private void Start()
    {
        _settingsMenuTransform.localScale = Vector3.zero;
        _settingsMenuTransform.gameObject.SetActive(false);
        _vsyncTick.SetActive(false);
    }

    private void OnSettingsButtonCliced()
    {
        if (_isAnimating) { return; }

        SetJoinCode();

        _isAnimating = true;
        _settingsMenuTransform.gameObject.SetActive(true);

        _backBackgroundImage.DOFade(0.8f, _animationDuration);

        _settingsMenuTransform.DOScale(1f, _animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _isAnimating = false;
        });
    }

    private void OnVsyncButtonClicked()
    {
        _isVsyncActive = !_isVsyncActive;
        QualitySettings.vSyncCount = _isVsyncActive ? 1 : 0;
        _vsyncTick.SetActive(_isVsyncActive);
    }

    private void OnLeaveGameButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.ShutDown();
        }

        ClientSinglleton.Instance.ClientGameManager.Disconnect();
    }

    private void OnKeepPlayingButtonClicked()
    {
        if (_isAnimating) { return; }

        _isAnimating = true;

        _backBackgroundImage.DOFade(0f, _animationDuration);
        _settingsMenuTransform.DOScale(0f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _isAnimating = false;
            _settingsMenuTransform.gameObject.SetActive(false);
            _isCopiedJoinCode = false;
            _copiedImage.sprite = _crossSprite;
        });
    }

    private void OnCopyCodeButtonCliced()
    {
        if (_isCopiedJoinCode) { return; }

        _isCopiedJoinCode = true;
        _copiedImage.sprite = _tickSprite;
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void SetJoinCode()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCodeText.text = HostSingleton.Instance.HostGameManager.GetJoinCode();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            _joinCodeText.text = ClientSinglleton.Instance.ClientGameManager.GetJoinCode();
        }
    }
}
