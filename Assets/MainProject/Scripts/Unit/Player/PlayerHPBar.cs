using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    public Slider slider;

    public void SetHP(float cur, float max)
    {
        slider.value = cur / max;
    }
}
