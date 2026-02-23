using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneTransitionController.Instance.LoadScene(sceneName);
    }
}
