using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float limitTime = 1800f; // 30분
    public TextMeshProUGUI timerText;

    [SerializeField] private EndGameUI End;
    [SerializeField] private SkillController skillController;

    private float remainTime;
    private bool finished;

    private void Start()
    {
        remainTime = limitTime;
        UpdateUI();
    }

    private void Update()
    {
        if (finished) return;
        if (Time.timeScale == 0) return; // 레벨업 중 정지

        remainTime -= Time.deltaTime;

        if (PlayerControll.Player.Instance.isDead)
        {
            FinishGame(true);
        }
        else
        {
            if (remainTime <= 0f)
            {
                remainTime = 0f;
                FinishGame(false);
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        int min = Mathf.FloorToInt(remainTime / 60);
        int sec = Mathf.FloorToInt(remainTime % 60);
        timerText.SetText("{0:00}:{1:00}", min, sec);
    }
    
    private void FinishGame(bool isGameOver)
    {
        finished = true;

        End.Open(
            !isGameOver,
            limitTime - remainTime,
            PlayerControll.Player.Instance.Level,
            KillManager.Instance.KillCount,
            skillController.GetSkillResults(),
            0.2f
        );
    }

}
