using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;


public class ValuePicker : MonoBehaviour
{
    public Image image;
    Color _color;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            showColorImage.color = Color;
            VoxelManager.color = Color;
        }
    }
    Slider slider;
    float value;
    float hue, sat;
    public Image showColorImage;

    int w, h;

    void Awake()
    {
        w = (int)GetComponent<RectTransform>().sizeDelta.x;
        h = (int)GetComponent<RectTransform>().sizeDelta.y;
        slider = GetComponent<Slider>();
        value = slider.value;
    } 

    public void SetValue()
    {
        // called from slider
        value = slider.value;
        Color = Color.HSVToRGB(hue, sat, value);
    }
    public void GenerateImage(Color color)
    {
        Texture2D texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
        float val = 0;
        UnityEngine.Color.RGBToHSV(color, out hue, out sat, out val);
        Color mainColor = color;
        Color secColor = UnityEngine.Color.black;
        Color = color;
        
        float f = 0;
        for (int x = 0; x < w; x++)
        {
            f = (float)x / (float)w;
            for (int y = 0; y < h; y++)
            {
                texture.SetPixel(x, y, UnityEngine.Color.Lerp(secColor, mainColor, f));
            }
        }

        texture.Apply();

        Sprite mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        mySprite.texture.wrapMode = TextureWrapMode.Repeat;
        image.sprite = mySprite;

        SetValue();
    }
}