using TMPro;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    [Header("이름 설정 팝업")]
    [SerializeField] GameObject setNamePopup;
    [SerializeField] TMP_InputField nameInput;

    [Header("씬")]
    [SerializeField] string multiplayerLobbySceneName;

    private void Start()
    {
        if (setNamePopup) setNamePopup.SetActive(false);

        LocalProfile.ClearAllForTest();
    }

    public void OnClickMultiplayer()
    {
        if (!LocalProfile.HasName)
        {
            OpenSetNamePopup();
            return;
        }

        SceneTransitionController.Instance.LoadScene(multiplayerLobbySceneName);
    }
    
    public void OnClickSingle(string sceneName)
    {
        SceneTransitionController.Instance.LoadScene(sceneName);
    }

    public void OpenSetNamePopup()
    {
        setNamePopup.SetActive(true);
        nameInput.text = "";
        nameInput.ActivateInputField();
    }

    public void ConfirmSetName()
    {
        string nick = nameInput.text.Trim();
        if (string.IsNullOrEmpty(nick))
        {
            Debug.Log("닉네임을 입력하세요");
            return;
        }

        LocalProfile.Name = nick;
        Debug.Log($"프로필 생성 완료 | id={LocalProfile.Id}, name={LocalProfile.Name}");

        setNamePopup.SetActive(false);

        SceneTransitionController.Instance.LoadScene(multiplayerLobbySceneName);
    }

    public void CloseSetNamePopup()
    {
        setNamePopup.SetActive(false);
    }
}