using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public Slider hpSlider;
    public Slider expSlider;

    public TextMeshProUGUI levelText;

    public void SetHP(float cur, float max)
    {
        hpSlider.value = cur / max;
    }

    private int cachedLevel = -1;

    public void SetExp(float cur, float max)
    {
        expSlider.value = cur / max;
    }

    public void SetLevel(int level)
    {
        if (cachedLevel == level) return;

        cachedLevel = level;
        levelText.SetText("Lv {0}", level);
    }
}
