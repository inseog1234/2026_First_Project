using TMPro;
using UnityEngine;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameTMP;

    public void SetName(string name)
    {
        if (nameTMP) nameTMP.text = name;
    }
}