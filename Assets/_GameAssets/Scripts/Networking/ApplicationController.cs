using Cysharp.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSinglleton _clientSinglletonPrefab;
    [SerializeField] private HostSingleton _hostSingletonPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async UniTask LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            // DETICATED SERVER
        }
        else
        {
            HostSingleton _hostSingletonInstance = Instantiate(_hostSingletonPrefab);
            _hostSingletonInstance.CreateHost();

            ClientSinglleton clientSingletonInstance = Instantiate(_clientSinglletonPrefab);
            bool isAuthenticate = await clientSingletonInstance.CreateClient();

            if (isAuthenticate)
            {
                clientSingletonInstance.ClientGameManager.GoToMainManu();
            }
        }
    }
}
