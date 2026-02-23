using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ChatLogTMP : MonoBehaviour
{
    [SerializeField] TMP_Text chatTMP;
    [SerializeField] int maxLines;

    readonly Queue<string> lines = new();
    readonly StringBuilder sb = new(4096);

    void Reset()
    {
        chatTMP = GetComponent<TMP_Text>();
    }

    public void AddLine(string richTextLine)
    {
        lines.Enqueue(richTextLine);

        while (lines.Count > maxLines)
            lines.Dequeue();

        sb.Clear();
        foreach (var l in lines)
            sb.AppendLine(l);

        if (chatTMP) chatTMP.SetText(sb.ToString());
    }

    public void AddSystem(string msg)
    {
        // 노랑색
        AddLine($"<color=#FFD966>{msg}</color>");
    }

    public void AddChat(string name, string msg)
    {
        AddLine($"{Escape(name)} : {Escape(msg)}");
    }

    static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("<", "&lt;").Replace(">", "&gt;");
    }
}