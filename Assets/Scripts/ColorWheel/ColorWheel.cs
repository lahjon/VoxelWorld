using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class ColorWheel : MonoBehaviour
{
    public Image image;
    public Button button;
    public RectTransform picker;
    bool holding;
    int height, width;

    void Awake()
    {
        
    }
    

    void Start()
    {
        int size = 512;
        Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

        var circle = DrawCircle(128);
        for (int i = 0; i < circle.Length; i++)
        {
            Debug.Log(circle[i]);
            texture.SetPixel(i, i, circle[i]);
        }
        

        texture.Apply();

        Sprite mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        image.sprite = mySprite;
    }

    void Update()
    {
        if (holding)
        {
            picker.position = Input.mousePosition;
        }
    }

    public void SetPickerPosition()
    {
        Debug.Log(image.GetComponent<RectTransform>().anchoredPosition);
        picker.position = Input.mousePosition;
        Debug.Log(picker.localPosition.normalized);
    }

    public void StartHold()
    {
        Debug.Log("Holding");
        holding = true;
    }
    public void StopHold()
    {
        holding = false;
        Debug.Log("Stop");
    }

    Color[] DrawCircle(int size)
    {
        Color[] row = new Color[size];
        Color[] column = new Color[size];
        float fSize = (float)size;
        float radius = fSize / 2f;

        for (int yIdx = 0; yIdx < size; yIdx++)
        {
            float y = yIdx - radius;
            for (int xIdx = 0; xIdx < size; xIdx++)
            {
                float x = xIdx - radius;
                float theta = Mathf.Atan2(x, y) - 3f * Mathf.PI / 2f;
                if (theta < 0)
                {
                    theta += 2f * Mathf.PI;
                }
                float r = Mathf.Sqrt(x * x + y * y);
                float hue = theta / (2f * Mathf.PI);
                float sat = Mathf.Min((r / radius), 1f);
                float val = 1f;
                row[xIdx] = Color.HSVToRGB(hue, sat, val);
                row[xIdx].a = 1f;
            }
            //column[yIdx] = row[xIdx];
        }
        return row;
    }
}

// BufferedImage.TYPE_INT_ARGB);
//   int[] row = new int[SIZE];
//   float size = (float) SIZE;
//   float radius = size / 2f;

//   for (int yidx = 0; yidx < SIZE; yidx++) {
//     float y = yidx - size / 2f;
//     for (int xidx = 0; xidx < SIZE; xidx++) {
//       float x = xidx - size / 2f;
//       double theta = Math.atan2(y, x) - 3d * Math.PI / 2d;
//       if (theta < 0) {
//         theta += 2d * Math.PI;
//       }
//       double r = Math.sqrt(x * x + y * y);
//       float hue = (float) (theta / (2d * Math.PI));
//       float sat = Math.min((float) (r / radius), 1f);
//       float bri = 1f;
//       row[xidx] = Color.HSBtoRGB(hue, sat, bri);
//     }
//     image.getRaster().setDataElements(0, yidx, SIZE, 1, row);
//   }
//   return image;
// }