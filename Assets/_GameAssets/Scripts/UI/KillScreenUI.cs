using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class KillScreenUI : MonoBehaviour
{
    public static KillScreenUI Instance { get; private set; }

    public event Action OnRespawnTimerFinished;

    [Header("Smash UI")]
    [SerializeField] private RectTransform _smashUITransfrom;
    [SerializeField] private TMP_Text _smashedPlayerText;

    [Header("Smashed UI")]
    [SerializeField] private RectTransform _smashedUITransfrom;
    [SerializeField] private TMP_Text _smashedByPlayerText;
    [SerializeField] private TMP_Text _respawnTimerText;

    [Header("Settings")]
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _smashUIStayDuration;

    private float _timer;
    private bool _isTimerActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _smashUITransfrom.transform.gameObject.SetActive(false);
        _smashedUITransfrom.transform.gameObject.SetActive(false);

        _smashUITransfrom.localScale = Vector3.zero;
        _smashedUITransfrom.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (_isTimerActive)
        {
            _timer -= Time.deltaTime;
            int timer = (int)_timer;
            _respawnTimerText.text = timer.ToString();

            if (timer <= 0f)
            {
                _smashedUITransfrom.localScale = Vector3.zero;
                _smashedUITransfrom.gameObject.SetActive(false);
                _isTimerActive = false;
                _smashedByPlayerText.text = string.Empty;
                OnRespawnTimerFinished?.Invoke();
            }
        }    
    }

    public void SetSmashUI(string playerNmae)
    {
        StartCoroutine(SetSmashUICorotine(playerNmae));
    }

    private IEnumerator SetSmashUICorotine(string playerName)
    {
        _smashUITransfrom.gameObject.SetActive(true);
        _smashedUITransfrom.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
        _smashedPlayerText.text = playerName;

        yield return new WaitForSeconds(_smashUIStayDuration);

        _smashUITransfrom.gameObject.SetActive(false);
        _smashUITransfrom.localScale = Vector3.zero;
        _smashedPlayerText.text = string.Empty;
    }

    public void SetSmashedUI(string playerNmae, int respawnTimer)
    {
        _smashedUITransfrom.transform.gameObject.SetActive(true);
        _smashedUITransfrom.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);

        _smashedByPlayerText.text = playerNmae;
        _respawnTimerText.text = respawnTimer.ToString();

        _isTimerActive = true;
        _timer = respawnTimer;
    }
}
