using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainManuUI : MonoBehaviour
{
    [Header("Referemce")]
    [SerializeField] private LobbiesListUI _lobbiesListUI;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _lobbiesButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private RectTransform _lobbiesBackgroundTransform;
    [SerializeField] private GameObject _lobbiesParentObject;
    [SerializeField] private TMP_InputField _joinCodeInputField;
    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private void Awake()
    {
        _hostButton.onClick.AddListener(StartHost);
        _clientButton.onClick.AddListener(StartClient);
        _lobbiesButton.onClick.AddListener(OpenLobbies);
        _closeButton.onClick.AddListener(CloseLobbies);
    }
    private void Start()
    {
        _lobbiesParentObject.SetActive(false);
    }

    private void OpenLobbies()
    {
        _lobbiesParentObject.SetActive(true);
        _lobbiesBackgroundTransform.DOAnchorPosX(-650f, _animationDuration).SetEase(Ease.OutBack);

        _lobbiesListUI.RefreshList();
    }

    private void CloseLobbies()
    {
        _lobbiesBackgroundTransform.DOAnchorPosX(900f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _lobbiesParentObject.SetActive(false);
        });
    }

    private async void StartHost()
    {
        await HostSingleton.Instance.HostGameManager.StartHostAsync();
    }

    private async void StartClient()
    {
        await ClientSinglleton.Instance.ClientGameManager.StartClientAsync(_joinCodeInputField.text);
    }
}
