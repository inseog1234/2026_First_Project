using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectButton : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI tagText;

    private SkillData data;
    private SkillSelectUI ui;

    public void Set(SkillData data, SkillSelectUI ui)
    {
        this.data = data;
        this.ui = ui;

        icon.sprite = data.icon;
        nameText.SetText(data.skillName);

        SetTag();
    }

    private void SetTag()
    {
        SkillController sc = ui.GetSkillController();

        if (!sc.HasSkill(data))
        {
            tagText.gameObject.SetActive(true);
            tagText.SetText("NEW");
            tagText.color = Color.yellow;
        }
        else
        {
            int cur = sc.GetSkillLevel(data);
            int max = data.maxLevel;

            if (cur < max)
            {
                tagText.gameObject.SetActive(true);

                if (cur + 1 >= max)
                {
                    tagText.SetText("Lv {0} -> MAX", cur);
                }
                else
                {
                    tagText.SetText("Lv {0} -> {1}", cur, cur+1);
                }

                tagText.color = Color.cyan;
            }
            else
            {
                tagText.gameObject.SetActive(false); // 만렙이면 표시 X
            }
        }
    }

    public void OnClick()
    {
        ui.OnSelectSkill(data);
    }
}
