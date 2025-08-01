using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int _colorId;
    [SerializeField] private Image _colorImage;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _selectedGameObject;

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            MultiplayerGameManager.Instance.ChangePlayerColor(_colorId);
        });
    }

    private void Start()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetwrokListChange += MultiplayerGameManager_OnPlayerDataNetwrokListChange;

        _colorImage.color = MultiplayerGameManager.Instance.GetPlayerColor(_colorId);
        UpdateIsSelected();
    }

    private void MultiplayerGameManager_OnPlayerDataNetwrokListChange()
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (MultiplayerGameManager.Instance.GetPlayerData().ColorId == _colorId)
        {
            _selectedGameObject.SetActive(true);
        }
        else
        {
            _selectedGameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetwrokListChange -= MultiplayerGameManager_OnPlayerDataNetwrokListChange;        
    }
}
