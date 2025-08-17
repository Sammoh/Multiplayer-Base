using System;
using System.Collections.Generic;
using System.Text;
using DedicatedServer.Domain.Interfaces;
using DedicatedServer.Domain.ValueObjects;
using UnityEngine;

namespace DedicatedServer.Infrastructure.Configuration
{
    /// <summary>
    /// Unity-specific implementation of configuration service.
    /// Handles command line arguments and Unity-specific settings.
    /// </summary>
    public class UnityConfigurationService : IConfigurationService
    {
        private readonly Dictionary<string, Action<string>> _commandDictionary;
        private const string k_IPCmd = "ip";
        private const string k_PortCmd = "port";
        private const string k_QueryPortCmd = "queryPort";

        public bool IsHeadlessMode => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;

        public UnityConfigurationService()
        {
            _commandDictionary = new Dictionary<string, Action<string>>
            {
                ["-" + k_IPCmd] = SetIP,
                ["-" + k_PortCmd] = SetPort,
                ["-" + k_QueryPortCmd] = SetQueryPort
            };

            // Set defaults
            SetIP("127.0.0.1");
            SetPort("7777");
            SetQueryPort("7787");
            
            ProcessCommandLineArguments(Environment.GetCommandLineArgs());
        }

        public ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(
                serverIP: PlayerPrefs.GetString(k_IPCmd, "127.0.0.1"),
                serverPort: PlayerPrefs.GetInt(k_PortCmd, 7777),
                queryPort: PlayerPrefs.GetInt(k_QueryPortCmd, 7787)
            );
        }

        public void SetServerConfiguration(string ip, int port, int queryPort)
        {
            PlayerPrefs.SetString(k_IPCmd, ip);
            PlayerPrefs.SetInt(k_PortCmd, port);
            PlayerPrefs.SetInt(k_QueryPortCmd, queryPort);
        }

        public string[] GetCommandLineArguments()
        {
            return Environment.GetCommandLineArgs();
        }

        private void ProcessCommandLineArguments(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Launch Args: ");
            
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var nextArg = "";
                if (i + 1 < args.Length)
                    nextArg = args[i + 1];

                if (EvaluatedArgs(arg, nextArg))
                {
                    sb.Append(arg);
                    sb.Append(" : ");
                    sb.AppendLine(nextArg);
                    i++;
                }
            }

            Debug.Log(sb);
        }

        private bool EvaluatedArgs(string arg, string nextArg)
        {
            if (!IsCommand(arg))
                return false;
            if (IsCommand(nextArg))
                return false;

            _commandDictionary[arg].Invoke(nextArg);
            return true;
        }

        private void SetIP(string ipArgument)
        {
            PlayerPrefs.SetString(k_IPCmd, ipArgument);
        }

        private void SetPort(string portArgument)
        {
            if (int.TryParse(portArgument, out int parsedPort))
            {
                PlayerPrefs.SetInt(k_PortCmd, parsedPort);
            }
            else
            {
                Debug.LogError($"{portArgument} does not contain a parseable port!");
            }
        }

        private void SetQueryPort(string qPortArgument)
        {
            if (int.TryParse(qPortArgument, out int parsedQPort))
            {
                PlayerPrefs.SetInt(k_QueryPortCmd, parsedQPort);
            }
            else
            {
                Debug.LogError($"{qPortArgument} does not contain a parseable query port!");
            }
        }

        private bool IsCommand(string arg)
        {
            return !string.IsNullOrEmpty(arg) && _commandDictionary.ContainsKey(arg) && arg.StartsWith("-");
        }
    }
}