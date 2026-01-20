using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillStatRow : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI damagePerSecText;

    public void PlayCountUp(SkillResultData data, float Timeoffset)
    {
        StopAllCoroutines();
        StartCoroutine(Sequence(data, Timeoffset));
    }

    private IEnumerator Sequence(SkillResultData data, float Timeoffset)
    {
        Icon.sprite = data.Icon;
        nameText.SetText(data.name);

        yield return CountUpInt(levelText, data.level+1);
        // yield return new WaitForSecondsRealtime(Timeoffset);

        yield return CountUpDouble(damageText, data.totalDamage);
        // yield return new WaitForSecondsRealtime(Timeoffset);

        yield return CountUpTime(timeText, data.Time);
        // yield return new WaitForSecondsRealtime(Timeoffset);

        double dps = data.totalDamage / Mathf.Max(data.Time, 0.01f);
        yield return CountUpDouble(damagePerSecText, dps);
    }


    private IEnumerator CountUpInt(TextMeshProUGUI t, int to)
    {
        float dur = 0.5f;
        float cur = 0f;

        while (cur < dur)
        {
            cur += Time.unscaledDeltaTime;
            int v = Mathf.RoundToInt(Mathf.Lerp(0, to, cur / dur));
            t.SetText("{0}", v);
            yield return null;
        }

        t.SetText("{0}", to);
    }

    private IEnumerator CountUpDouble(TextMeshProUGUI t, double to)
    {
        float dur = 0.7f;
        float cur = 0f;

        while (cur < dur)
        {
            cur += Time.unscaledDeltaTime;
            double v = Mathf.Lerp(0f, (float)to, cur / dur);
            t.text = FormatBig(v);
            yield return null;
        }

        t.text = FormatBig(to);
    }

    private IEnumerator CountUpTime(TextMeshProUGUI t, float to)
    {
        float dur = 0.6f;
        float cur = 0f;

        while (cur < dur)
        {
            cur += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(0f, to, cur / dur);

            int m = (int)(v / 60);
            int s = (int)(v % 60);
            t.SetText("{0:0}:{1:00}", m, s);

            yield return null;
        }

        int fm = (int)(to / 60);
        int fs = (int)(to % 60);
        t.SetText("{0:0}:{1:00}", fm, fs);
    }

    private string FormatBig(double v)
    {
        if (v >= 1_000_000_000) return $"<color=#FF5555>{v / 1e9:0.#}B</color>";
        if (v >= 1_000_000)     return $"<color=#FFAA00>{v / 1e6:0.#}M</color>";
        if (v >= 1_000)         return $"<color=#55AAFF>{v / 1e3:0.#}K</color>";
        return v.ToString("0");
    }
}
