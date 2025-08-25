using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCanvasManager : MonoBehaviour
{
    [SerializeField] GameObject levelPanel;
    void Start()
    {
        levelPanel.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void GoToLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
}
