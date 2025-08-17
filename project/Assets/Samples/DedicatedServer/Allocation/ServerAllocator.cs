using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;

public class ServerAllocator : MonoBehaviour
{
    [SerializeField] string fleetId = "YOUR_FLEET_ID";
    [SerializeField] string buildConfigurationId = "12345"; // or numeric
    [SerializeField] string regionId = "us-west1";

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        var args = new Dictionary<string, object>
        {
            { "fleetId", fleetId },
            { "buildConfigurationId", buildConfigurationId },
            { "regionId", regionId },
            { "waitForReady", true }, // set false to return immediately
            { "payload", new Dictionary<string, object> {
                { "map", "Arena01" },
                { "allowedPlayers", new [] { AuthenticationService.Instance.PlayerId } }
            } }
        };

        var result = await CloudCodeService.Instance
            .CallEndpointAsync<AllocationResult>("RequestServerAllocation", args);

        if (result.pending)
        {
            Debug.Log($"Allocation queued: {result.allocationId}. Poll later for IP/port.");
            return;
        }

        Debug.Log($"Allocated: {result.server.ip}:{result.server.port}");
        // Set UnityTransport connection data and StartClient();
    }

    [System.Serializable]
    public class AllocationResult
    {
        public string allocationId;
        public bool pending;
        public ServerInfo server;
        public string href;
    }

    [System.Serializable]
    public class ServerInfo { public string ip; public int port; }
}