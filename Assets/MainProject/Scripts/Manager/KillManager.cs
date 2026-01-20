using UnityEngine;
using TMPro;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    public TextMeshProUGUI killText;
    private int killCount;
    public int KillCount => killCount;
    
    private void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddKill(int count = 1)
    {
        killCount += count;
        UpdateUI();
    }

    private void UpdateUI()
    {
        killText.SetText("킬 수 : {0}", killCount);
    }
}
