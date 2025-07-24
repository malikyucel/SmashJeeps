using DG.Tweening;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [Header("Reference")]
    [SerializeField] private RectTransform _healthBarTransform;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHealth(int health, int maxHealth)
    {
        _healthBarTransform.DOScaleX(health / maxHealth, _animationDuration).SetEase(Ease.Linear);
    }
}
