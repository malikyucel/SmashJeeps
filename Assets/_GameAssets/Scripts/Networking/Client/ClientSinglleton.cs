using Cysharp.Threading.Tasks;
using UnityEngine;

public class ClientSinglleton : MonoBehaviour
{
    private static ClientSinglleton instance;

    public ClientGameManager ClientGameManager { get; private set; }

    public static ClientSinglleton Instance
    {
        get
        {
            if (Instance != null) { return instance; }

            instance = FindAnyObjectByType<ClientSinglleton>();

            if (instance == null)
            {
                Debug.LogError("No CliendSingleton in the scene!");
            }

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async UniTask<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();
        return await ClientGameManager.InitAsync();
    }
}
