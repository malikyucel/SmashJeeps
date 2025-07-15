using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelectorUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private Button _connectButton;

    private void Awake()
    {
        _connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(Const.SceneName.LOADING_SCENE);
            return;
        }

        _nameInputField.text = PlayerPrefs.GetString(Const.PlayerData.PLAYER_NAME, string.Empty);        
    }

    private void OnConnectButtonClicked()
    {
        PlayerPrefs.SetString(Const.PlayerData.PLAYER_NAME, _nameInputField.text);
        SceneManager.LoadScene(Const.SceneName.LOADING_SCENE);
    }
}
