using UnityEngine;

[CreateAssetMenu(menuName = "WeZard/SupabaseConfig")]
public class SupabaseConfig : ScriptableObject
{
    public string supabaseUrl;
    public string anonKey;
}