using System;
using UnityEngine;

public static class LocalProfile
{
    private const string KEY_ID = "WZ_PROFILE_ID";
    private const string KEY_NAME = "WZ_PROFILE_NAME";

    public static string Id
    {
        get
        {
            if (!PlayerPrefs.HasKey(KEY_ID))
            {
                PlayerPrefs.SetString(KEY_ID, Guid.NewGuid().ToString());
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString(KEY_ID);
        }
    }

    public static bool HasName => PlayerPrefs.HasKey(KEY_NAME) && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(KEY_NAME));

    public static string Name
    {
        get => PlayerPrefs.GetString(KEY_NAME, "");
        set
        {
            PlayerPrefs.SetString(KEY_NAME, value);
            PlayerPrefs.Save();
        }
    }

    public static void ClearAllForTest()
    {
        PlayerPrefs.DeleteKey(KEY_ID);
        PlayerPrefs.DeleteKey(KEY_NAME);
        PlayerPrefs.Save();
    }
}