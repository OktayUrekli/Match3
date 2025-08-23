using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Level level;

    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI scoreText;

    public Image[] emptyStars;
    public Image[] fillStars;


    int starIndex;
    bool isGameOver=false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
