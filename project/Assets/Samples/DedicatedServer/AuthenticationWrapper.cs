using System;
using System.Threading.Tasks;
using Matchplay.Shared;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimedOut
    }

    public static class AuthenticationWrapper
    {
        public static AuthState AuthorizationState { get; private set; } = AuthState.NotAuthenticated;

        public static async Task<AuthState> DoAuth(int tries = 5)
        {
            //If we are already authenticated, just return Auth
            if (AuthorizationState == AuthState.Authenticated)
            {
                Debug.Log("Already Authenticated");
                return AuthorizationState;
            }

            if (AuthorizationState == AuthState.Authenticating)
            {
                Debug.LogWarning("Cant Authenticate if we are authenticating or authenticated");
                await Authenticating();
                return AuthorizationState;
            }

            await SignInAnonymouslyAsync(tries);
            Debug.Log($"Auth attempts Finished : {AuthorizationState.ToString()}");

            return AuthorizationState;
        }

        //Awaitable task that will pass the clientID once authentication is done.
        public static string PlayerID()
        {
            return AuthenticationService.Instance.PlayerId;
        }

        //Awaitable task that will pass once authentication is done.
        public static async Task<AuthState> Authenticating()
        {
            while (AuthorizationState == AuthState.Authenticating || AuthorizationState == AuthState.NotAuthenticated)
            {
                await Task.Delay(200);
            }

            return AuthorizationState;
        }

        // static async Task SignInAnonymouslyAsync(int maxRetries)
        // {
        //     Debug.Log("SignInAnonymouslyAsync");
        //     AuthorizationState = AuthState.Authenticating;
        //     var tries = 0;
        //     while (AuthorizationState == AuthState.Authenticating && tries < maxRetries)
        //     {
        //         try
        //         {
        //             //To ensure staging login vs non staging
        //             await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //             Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
        //
        //             if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
        //             {
        //                 AuthorizationState = AuthState.Authenticated;
        //                 Debug.Log($"Player signed in successfully: {AuthenticationService.Instance.PlayerId}");
        //                 break;
        //             }
        //         }
        //         catch (AuthenticationException ex)
        //         {
        //             // Compare error code to AuthenticationErrorCodes
        //             // Notify the player with the proper error message
        //             Debug.LogError(ex);
        //             AuthorizationState = AuthState.Error;
        //         }
        //         catch (RequestFailedException exception)
        //         {
        //             // Compare error code to CommonErrorCodes
        //             // Notify the player with the proper error message
        //             Debug.LogError(exception);
        //             AuthorizationState = AuthState.Error;
        //         }
        //
        //         tries++;
        //         await Task.Delay(1000);
        //         Debug.Log($"Tries : {tries}");
        //     }
        //
        //     if (AuthorizationState != AuthState.Authenticated)
        //     {
        //         Debug.LogError($"Player was not signed in successfully after {tries} attempts");
        //         AuthorizationState = AuthState.TimedOut;
        //     }
        // }
        //
        static async Task SignInAnonymouslyAsync(int maxRetries)
        {
            Debug.Log("Initializing anonymous sign-in or session refresh...");
            AuthorizationState = AuthState.Authenticating;
            var tries = 0;

            while (AuthorizationState == AuthState.Authenticating && tries < maxRetries)
            {
                try
                {
                    // Reuse existing session token to refresh session/id tokens
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                    {
                        AuthorizationState = AuthState.Authenticated;
                        Debug.Log($"Signed in (or session refreshed) as {AuthenticationService.Instance.PlayerId}");
                        return;
                    }
                    Debug.LogWarning("Sign-in return did not authenticate—retrying...");
                }
                catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
                {
                    // Already signed in or in invalid state; clear stale token and retry cleanly
                    Debug.LogWarning("Invalid user state—clearing session token and retrying.");
                    AuthenticationService.Instance.ClearSessionToken();
                }
                catch (AuthenticationException authEx)
                {
                    Debug.LogError($"Auth error [{authEx.ErrorCode}]: {authEx.Message}");
                    AuthorizationState = AuthState.Error;
                    break;
                }
                catch (RequestFailedException rfEx)
                {
                    Debug.LogError($"Network/Request failure [{rfEx.ErrorCode}]: {rfEx.Message}");
                    // Optional: only break on critical error codes
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected failure: {ex.Message}");
                    AuthorizationState = AuthState.Error;
                    break;
                }

                tries++;
                Debug.Log($"Retry {tries}/{maxRetries} after delay...");
                await Task.Delay(1000);
            }

            if (AuthorizationState != AuthState.Authenticated)
            {
                Debug.LogError($"Failed to sign in after {tries} attempts.");
                AuthorizationState = AuthState.TimedOut;
            }
        }

        public static void SignOut()
        {
            AuthenticationService.Instance.SignOut(false);
            AuthorizationState = AuthState.NotAuthenticated;
        }
    }
}