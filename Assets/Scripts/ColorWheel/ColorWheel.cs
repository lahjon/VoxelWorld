using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;
using System;

public class ColorWheel : MonoBehaviour
{
    public Image image;
    public RectTransform picker;
    public VoxelManager voxelManager;
    public ValuePicker valuePicker;
    public bool Holding {get;set;}
    int height, width;
    int size;

    void Start()
    {
        size = 250;
        DrawCircle(size);
    }

    void Update()
    {
        if (Holding)
        {
            SetPickerPosition();
        }
    }

    public void SetPickerPosition()
    {
        // get position from picker and translate to pixel position
        picker.position = Input.mousePosition;
        Vector3Int pixelPos = Vector3Int.RoundToInt(new Vector3(
                picker.localPosition.x.Remap(-size/2, size/2, 0, size), 
                picker.localPosition.y.Remap(-size/2, size/2, 0, size), 
                picker.localPosition.z.Remap(-size/2, size/2, 0, size))
            );

        // get pixel color value and set new color
        valuePicker.GenerateImage(image.sprite.texture.GetPixel(pixelPos.x, pixelPos.y));
    }

    void DrawCircle(int size)
    {
        int radius = size / 2;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color emptyColor = new Color(0,0,0,0);
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                (float r, float deg) = XY2Polar(x, y);

                // coords adjusted to polar coords
                int adjustedX = x + radius;
                int adjustedY = y + radius;

                if (r > radius)
                {
                    // if outside of circle, set transparent
                    texture.SetPixel(adjustedX, adjustedY, emptyColor);
                    continue;
                }

                // remap degrees to 0-1 float range
                float hue = deg.Remap(-180, 180, 0, 1);
                // set saturation based on distance from center
                float sat = r / radius;
                // value always one in color wheel
                float val = 1f;
                
                Color color = Color.HSVToRGB(hue, sat, val);
                texture.SetPixel(adjustedX, adjustedY, color);
            }
        }
        
        texture.Apply();
        Sprite mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        image.sprite = mySprite;

        valuePicker.GenerateImage(Color.white);
    }

    (float r, float deg) XY2Polar(int x, int y)
    {
        // distance from center to point
        float r = Mathf.Sqrt(x*x + y*y);
        // angle of point
        float deg = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (r, deg);
    }

}

public static class ExtensionMethods 
{
    public static float Remap (this float value, float from1, float to1, float from2, float to2) 
    {
        // remap range1 to range2, 1, 0, 2, 0, 10 = 5
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
