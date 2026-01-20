using System.Collections.Generic;
using UnityEngine;

public class SkillSelectUI : MonoBehaviour
{
    public SkillSelectButton[] buttons;
    private SkillController skillController;

    public SkillController GetSkillController()
    {
        return skillController;
    }

    public void Open(SkillController controller)
    {
        skillController = controller;

        List<SkillData> list = controller.GetRandomSkillList(buttons.Length);

        if (list.Count == 0)
            return;

        for (int i = 0; i < buttons.Length; i++)
            buttons[i].gameObject.SetActive(false);

        for (int i = 0; i < list.Count; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].Set(list[i], this);
        }

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnSelectSkill(SkillData data)
    {
        skillController.ApplySkill(data);
        Close();
    }

    private void Close()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
