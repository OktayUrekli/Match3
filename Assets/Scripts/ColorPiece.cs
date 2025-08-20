using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        YELLOW,
        RED,
        PURBLE,
        ORANGE,
        BLUE,
        GREEN,
        PINK,
        ANY,
        COUNT,
    };


    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;

    ColorType color;

    public ColorType Color {
        get { return color; }
        set { SetColor(value); }
    }

    public int NumColors { get { return colorSprites.Length; } }


    SpriteRenderer sprite;

    Dictionary<ColorType, Sprite> colorSpriteDict; 
    private void Awake()
    {
        sprite = transform.Find("Visual").GetComponent<SpriteRenderer>();

        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for (int i = 0; i < colorSprites.Length; i++) 
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color)) // eðer bu renkte bir key yoksa 
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite); // ekleme gerçekleþir
            }
        }
    }

    public void SetColor(ColorType newColor) 
    {
        color = newColor;

        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
}
