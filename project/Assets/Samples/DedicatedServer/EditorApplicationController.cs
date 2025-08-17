using System.Linq;
using Matchplay.Shared;
using UnityEngine;
// TODO
using Unity.Multiplayer.Playmode;

namespace Matchplay.Editor
{
    ///Helps launch ParrelSynced Projects for easy testing
    public class EditorApplicationController : MonoBehaviour
    {
        public ApplicationController m_Controller;

        public void Start()
        {
#if UNITY_EDITOR
            // TODO: Tags needs Playmode... Find out what we have now.
            var mppmTag = CurrentPlayer.ReadOnlyTags();
            
            foreach (var tag in mppmTag)
            {
                Debug.LogError($"Current Player Tag: {tag}");
            }
            
            // var networkManager = NetworkManager.Singleton;
            if (mppmTag.Contains("Server"))
            {
                Debug.LogError("Starting Server");
                m_Controller.OnParrelSyncStarted(true,"server");
            
            }
            else if (mppmTag.Contains("Host"))
            {
                Debug.LogError("Starting Host - NOT IMPLEMENTED");
            }
            else if (mppmTag.Contains("Client"))
            {
                Debug.LogError("Starting Client");
                m_Controller.OnParrelSyncStarted(false,"client");
            }
            
            // if (ClonesManager.IsClone())
            // {
            //     var argument = ClonesManager.GetArgument();
            //     if (argument == "server")
            //         m_Controller.OnParrelSyncStarted(true,"server");
            //     else if (argument == "client")
            //     {
            //         m_Controller.OnParrelSyncStarted(false,"client");
            //     }
            // }
            // else
            //     m_Controller.OnParrelSyncStarted(false, "client");
#endif
        }
    }
}