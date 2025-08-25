using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("On Game Pause Variables")]
    [SerializeField] GameObject pausePanel;

    [Header("On Game Win Variables")]
    [SerializeField] GameObject gameWinPanel;
    [SerializeField] Image[] fillStars;
    [SerializeField] TextMeshProUGUI scoreTextOnWin;

    [Header("On Game Loose Variables")]
    [SerializeField] GameObject gameLoosePanel;
    [SerializeField] TextMeshProUGUI scoreTextOnLoose;

    void Start()
    {
        gameLoosePanel.SetActive(false);
        gameWinPanel.SetActive(false);
        pausePanel.SetActive(false);

        for (int i = 0; i < fillStars.Length; i++)
        {
            fillStars[i].gameObject.SetActive(false);
        }
    }

    public void OnGameWin(int score,int starCount)
    {
        if (gameWinPanel != null && gameLoosePanel != null)
        {
            gameWinPanel.SetActive(true);
            gameLoosePanel.SetActive(false);

            for (int i = 0; i < starCount; i++)
            {
                fillStars[i].gameObject.SetActive(true);
            }

            scoreTextOnWin.text = "SCORE: " + score.ToString();
        }
    }

    public void OnGameLoose(int score)
    {
        if (gameWinPanel != null && gameLoosePanel != null)
        {
            gameLoosePanel.SetActive(true);
            gameWinPanel.SetActive(false);
            scoreTextOnLoose.text="SCORE: "+score.ToString();
        }
    }

    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");

    }

    public void NextLevelButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);

    }

    public void PauseButton()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ContinueButton()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

    }



}
