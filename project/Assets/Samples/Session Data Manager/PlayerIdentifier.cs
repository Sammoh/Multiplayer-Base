using System;
using UnityEngine;

public static class PlayerIdentifier
{
    private const string PlayerIdKey = "PlayerId";

    public static string GetOrCreatePlayerId()
    {
        if (!PlayerPrefs.HasKey(PlayerIdKey))
        {
            var newId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PlayerIdKey, newId);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(PlayerIdKey);
    }
}