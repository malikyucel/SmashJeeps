using UnityEngine;
using UnityEngine.UI;

public class MainManuUI : MonoBehaviour
{
    [SerializeField] private Button _hostButton;

    private void Awake()
    {
        _hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private async void OnHostButtonClicked()
    {
        await HostSingleton.Instance.HostGameManager.StartHostAsync();  
    }
}
