using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public HostGameManager HostGameManager { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if (Instance != null) { return instance; }

            instance = FindAnyObjectByType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("No Hostingleton in the scene!");
            }

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost()
    {
        HostGameManager = new HostGameManager();
    }
}
