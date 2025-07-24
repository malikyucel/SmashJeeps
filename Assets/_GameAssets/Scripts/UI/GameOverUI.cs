using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private LeaderboardUI _leaderboardUI;
    [SerializeField] private ScoreTablePlayerUI _scoreTablePlayerPrefab;
    [SerializeField] private Transform _scoreTableParentTransform;
    [SerializeField] private Image _gameOverBackgroundImage;
    [SerializeField] private RectTransform _gameOverTransform;
    [SerializeField] private RectTransform _scoreTableTransform;
    [SerializeField] private TMP_Text _winnerText;
    [SerializeField] private Button _mainManuButton;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _scaleDuration;

    private RectTransform _mainManuButtonTransform;
    private RectTransform _winnerTransform;

    private void Awake()
    {
        _mainManuButton.onClick.AddListener(OnMainManuButtonClicked);

        _mainManuButtonTransform = _mainManuButton.GetComponent<RectTransform>();
        _winnerTransform = _winnerText.GetComponent<RectTransform>();
    }

    private void OnMainManuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.ShutDown();
        }

        ClientSinglleton.Instance.ClientGameManager.Disconnect();
    }

    private void Start()
    {
        _scoreTableTransform.gameObject.SetActive(false);
        _scoreTableTransform.localScale = Vector3.zero;

        GameManager.Instance.OnGameSataeChange += GaemManager_OnGameStateChange;
    }

    private void GaemManager_OnGameStateChange(GaemState gaemState)
    {
        if (gaemState == GaemState.GameOver)
        {
            AnimateGameOver();
        }
    }

    private void AnimateGameOver()
    {
        _gameOverBackgroundImage.DOFade(0.8f, _animationDuration / 2);
        _gameOverTransform.DOAnchorPosY(0f, _animationDuration).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            _gameOverTransform.GetComponent<TMP_Text>().DOFade(0f, _animationDuration / 2).SetDelay(1f).OnComplete(() =>
            {
                AnimateLeaderboardAndButtons();
            });
        });
    }

    private void AnimateLeaderboardAndButtons()
    {
        _scoreTableTransform.gameObject.SetActive(true);
        _scoreTableTransform.DOScale(0.8f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _mainManuButtonTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _winnerTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
            });
        });

        PopulateGameOvnerLeaderboard();
    }

    private void PopulateGameOvnerLeaderboard()
    {
        var leaderboardData = _leaderboardUI.GetLeaderboardData().OrderByDescending(x => x.Score).ToList();

        HashSet<ulong> existingCliemtIds = new HashSet<ulong>();

        for (int i = 0; i < leaderboardData.Count; i++)
        {
            var entry = leaderboardData[i];

            if (existingCliemtIds.Contains(entry.ClientId))
            {
                continue;
            }

            ScoreTablePlayerUI scoreTablePlayerInstance = Instantiate(_scoreTablePlayerPrefab, _scoreTableParentTransform);
            bool isOwner = entry.ClientId == NetworkManager.Singleton.LocalClientId;
            int rank = i + 1;

            scoreTablePlayerInstance.SetScoreTableData(rank.ToString(), entry.PlayerName, entry.Score.ToString(), isOwner);

            existingCliemtIds.Add(entry.ClientId);
        }

        SetWinnerName();
    }

    private void SetWinnerName()
    {
        string winnerName = _leaderboardUI.GetWinnersName();
        _winnerText.text = winnerName + " SMASHED Y'ALL!";
    }
}
