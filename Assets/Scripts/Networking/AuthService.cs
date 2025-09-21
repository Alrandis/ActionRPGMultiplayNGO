using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    public async Task SignInAnonymously()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log($"Signed in! PlayerID: {AuthenticationService.Instance.PlayerId}");
    }
}
