using Matchplay.Shared;
using UnityEngine;

namespace Matchplay.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        [field: Scene]
        public string loadSceneName;
        
        public static ClientSingleton Instance
        {
            get
            {
                if (s_ClientGameManager != null) return s_ClientGameManager;
                s_ClientGameManager = FindFirstObjectByType<ClientSingleton>();
                if (s_ClientGameManager == null)
                {
                    Debug.LogError("No ClientSingleton in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return s_ClientGameManager;
            }
        }

        static ClientSingleton s_ClientGameManager;

        public ClientGameManager Manager
        {
            get
            {
                if (clientGameManager != null) return clientGameManager;
                Debug.LogError($"ClientGameManager is missing, did you run StartClient()?", gameObject);
                return null;
            }
        }

        ClientGameManager clientGameManager;

        public void CreateClient(string profileName = "default")
        {
            clientGameManager = new ClientGameManager(profileName);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            
            var guid = System.Guid.NewGuid();
            NameGenerator.GetName(guid.ToString());
            CreateClient();
        }
        
        void OnDestroy()
        {
            Manager?.Dispose();
        }
    }
}