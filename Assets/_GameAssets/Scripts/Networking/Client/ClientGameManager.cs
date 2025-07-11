using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    public async UniTask<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthenticationState authenticationState = await AuthenticationHandle.DoAuth();

        if (authenticationState == AuthenticationState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMainManu()
    {
        SceneManager.LoadScene(Const.SceneName.MANU_SCENE);
    }
}
