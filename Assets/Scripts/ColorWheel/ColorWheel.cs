using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;
using System;
using UnityEngine.InputSystem;

public class ColorWheel : MonoBehaviour
{
    public Image image;
    public RectTransform picker;
    public ValuePicker valuePicker;
    int radius;
    bool _holding;
    public bool Holding 
    {
        get => _holding;
        set
        {
            if (_holding && !value)
            {
                VoxelManager.instance.palette.UpdateColor(valuePicker.Color);
            }
            _holding = value; 
        }
    }
    int size;

    void Start()
    {
        size = 250;
        radius = size / 2;
        DrawCircle();
    }

    void Update()
    {
        if (Holding)
        {
            InBoundry();
            SetPickerPositionFromMouse();
        }
    }

    void InBoundry()
    {
        picker.position = Mouse.current.position.ReadValue();
        (float r, float deg) = XY2Polar((int)picker.localPosition.x, (int)picker.localPosition.y);

        if (r > radius) picker.localPosition = ClosestPointOnCircle(picker.localPosition);
        
    }

    public void SetPickerPositionFromColor(Color aColor)
    {
        float hue, value, sat;
        Color color;
        
        Color.RGBToHSV(aColor, out hue,out sat,out value);
        aColor = Color.HSVToRGB(hue, sat, 1);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                color = image.sprite.texture.GetPixel(x, y);
                if (color == aColor)
                {
                    picker.localPosition = new Vector3(((float)x).Remap(0, size, -radius, radius), ((float)y).Remap(0, size, -radius, radius), picker.localPosition.z);
                    valuePicker.GenerateImage(aColor);
                    return;
                }
            }
        }
    }

    public void SetPickerPositionFromMouse()
    {
        // get position from picker and translate to pixel position
        Vector3Int pixelPos = Vector3Int.RoundToInt(new Vector3(
                picker.localPosition.x.Remap(-radius, radius, 0, size), 
                picker.localPosition.y.Remap(-radius, radius, 0, size), 
                picker.localPosition.z.Remap(-radius, radius, 0, size))
            );

        // get pixel color value and set new color
        valuePicker.GenerateImage(image.sprite.texture.GetPixel(pixelPos.x, pixelPos.y));
    }

    void DrawCircle()
    {
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

    Vector3 ClosestPointOnCircle(Vector3 point) => Vector3.Normalize(point) * (radius - .9f);

}
