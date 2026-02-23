using System;
using UnityEngine;

public static class PlayerIdentity
{
    const string KEY_ID = "WZ_PLAYER_ID";
    const string KEY_NAME = "WZ_PLAYER_NAME";

    public static string PlayerId
    {
        get
        {
            if (!PlayerPrefs.HasKey(KEY_ID))
                PlayerPrefs.SetString(KEY_ID, Guid.NewGuid().ToString());
            return PlayerPrefs.GetString(KEY_ID);
        }
    }

    public static string PlayerName
    {
        get => PlayerPrefs.GetString(KEY_NAME, "Player");
        set => PlayerPrefs.SetString(KEY_NAME, value);
    }
}