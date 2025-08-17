// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using Unity.Netcode;
//
// namespace DedicatedServer.Client
// {
//     public class ServerClientUI : MonoBehaviour
//     {
//         [Header("UI References")]
//         [SerializeField] private Button hostButton;
//         [SerializeField] private Button joinButton;
//         [SerializeField] private TMP_InputField ipInputField;
//
//         void Awake()
//         {
//             hostButton.onClick.AddListener(StartHost);
//             joinButton.onClick.AddListener(JoinServer);
//         }
//
//         private void StartHost()
//         {
//             NetworkManager.Singleton.StartServer();
//             Debug.Log("StartServer started.");
//         }
//
//         private void JoinServer()
//         {
//             string ipAddress = ipInputField.text;
//             if (string.IsNullOrEmpty(ipAddress))
//             {
//                 Debug.LogWarning("Please enter a valid IP address.");
//                 return;
//             }
//
//             NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
//                 .SetConnectionData(ipAddress, 7777);
//
//             // NetworkManager.Singleton.StartClient();
//             Debug.Log($"Attempting to join server at {ipAddress}.");
//         }
//     }
// }
//
// // Server Singleton initialization (ensure this runs on headless builds)
// namespace DedicatedServer.Server
// {
//     public class ServerInitializer : MonoBehaviour
//     {
//         void Start()
//         {
// #if UNITY_SERVER
//             StartServer();
// #endif
//         }
//
//         private void StartServer()
//         {
//             if (NetworkManager.Singleton.IsListening)
//             {
//                 Debug.LogWarning("Server is already running.");
//                 return;
//             }
//             
//             // hook into the NetworkManager events if needed
//             NetworkManager.Singleton.OnServerStarted += () =>
//             {
//                 Debug.Log("Server started successfully.");
//             };
//             NetworkManager.Singleton.OnServerStopped += r =>
//             {
//                 Debug.Log($"Server stopped. {r}");
//             };
//             NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
//             {
//                 Debug.Log($"Client connected: {clientId}");
//             };
//             NetworkManager.Singleton.OnClientDisconnectCallback += clientId =>
//             {
//                 Debug.Log($"Client disconnected: {clientId}");
//             };
//             // Start the server
//             if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
//             {
//                 Debug.LogWarning("Cannot start server while client or host is running.");
//                 return;
//             }  
//             if (NetworkManager.Singleton.IsServer)
//             {
//                 Debug.LogWarning("Server is already running.");
//                 return;
//             }
//             // // Ensure the transport is set up correctly
//             var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
//             if (transport == null)
//             {
//                 Debug.LogError("UnityTransport component is missing. Please add it to the NetworkManager.");
//                 return;
//             }
//             
//             NetworkManager.Singleton.StartServer();
//             Debug.Log("Server initialized.");
//         }
//     }
// }