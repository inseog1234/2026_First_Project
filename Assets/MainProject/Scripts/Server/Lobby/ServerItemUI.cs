using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerItemUI : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameTMP;
    [SerializeField] TMP_Text hostNameTMP;
    [SerializeField] TMP_Text countTMP;
    [SerializeField] GameObject lockIcon;
    [SerializeField] Button joinButton;

    public void Bind(string roomName, string hostName, bool isLocked, string countText, Action onJoin)
    {
        roomNameTMP.text = roomName;
        hostNameTMP.text = hostName;
        countTMP.text = countText;
        if (lockIcon) lockIcon.SetActive(isLocked);

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoin?.Invoke());
    }
}