using System;
using TMPro;
using UnityEngine;

public class HostChatClient : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_InputField input;
    [SerializeField] ChatLogTMP chatLog;

    void Awake()
    {
        input.onEndEdit.RemoveListener(OnEndEditSubmit);
        input.onEndEdit.AddListener(OnEndEditSubmit);
    }

    public void OnClickSend()
    {
        OnEndEditSubmit(input.text);
    }

    public void FocusInput()
    {
        input.ActivateInputField();
    }

    private async void OnEndEditSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        string msg = text.Trim();
        if (msg.Length == 0) return;

        input.text = "";
        input.ActivateInputField();

        if (RelayChatClient.Instance == null || !RelayChatClient.Instance.IsConnected)
        {
            chatLog?.AddSystem("릴레이 서버에 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(Session.CurrentRoomId))
        {
            chatLog?.AddSystem("방에 들어가야 채팅이 가능합니다.");
            return;
        }

        try
        {
            await RelayChatClient.Instance.SendChatAsync(Session.CurrentRoomId, LocalProfile.Id, msg);
        }
        catch (Exception e)
        {
            chatLog?.AddSystem("채팅 전송 실패: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (input != null) input.onEndEdit.RemoveListener(OnEndEditSubmit);
    }
}