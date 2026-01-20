using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI killText;

    public Transform skillListRoot;
    public SkillStatRow rowPrefab;

    public void Open(
        bool isClear,
        float surviveTime,
        int playerLevel,
        int killCount,
        List<SkillResultData> skills,
        float Timeoffset
    )
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        resultText.SetText(isClear ? "GAME CLEAR" : "GAME OVER");

        StopAllCoroutines();
        StartCoroutine(PlaySequence(
            surviveTime,
            playerLevel,
            killCount,
            skills,
            Timeoffset
        ));
    }

    private IEnumerator PlaySequence(
        float time,
        int level,
        int kills,
        List<SkillResultData> skills,
        float Timeoffset
    )
    {
        yield return CountUpTime(timeText, time);
        yield return new WaitForSecondsRealtime(Timeoffset);

        yield return CountUpInt(levelText, "{0}", level);
        yield return new WaitForSecondsRealtime(Timeoffset);

        yield return CountUpInt(killText, "{0}", kills);
        yield return new WaitForSecondsRealtime(Timeoffset);

        foreach (Transform c in skillListRoot)
            Destroy(c.gameObject);

        foreach (var s in skills)
        {
            SkillStatRow row = Instantiate(rowPrefab, skillListRoot);
            row.PlayCountUp(s, Timeoffset);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // =======================
    // 숫자 연출 함수들
    // =======================

    private IEnumerator CountUpInt(TextMeshProUGUI t, string format, int to)
    {
        float duration = 0.6f; // 연출 시간
        float tCur = 0f;

        while (tCur < duration)
        {
            tCur += Time.unscaledDeltaTime;
            int v = Mathf.RoundToInt(Mathf.Lerp(0, to, tCur / duration));
            t.SetText(format, v);
            yield return null;
        }

        t.SetText(format, to);
    }


    private IEnumerator CountUpTime(TextMeshProUGUI t, float to)
    {
        float v = 0f;
        while (v < to)
        {
            v += Time.unscaledDeltaTime * to * 0.6f;

            int m = (int)(v / 60);
            int s = (int)(v % 60);
            t.SetText("{0:00}:{1:00}", m, s);

            yield return null;
        }

        int min = (int)(to / 60);
        int sec = (int)(to % 60);
        t.SetText("{0:00}:{1:00}", min, sec);
    }
}
