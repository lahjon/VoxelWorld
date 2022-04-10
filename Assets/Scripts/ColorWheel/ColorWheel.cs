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

    void Awake()
    {
        
    }
    

    void Start()
    {
        Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

        texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
        texture.SetPixel(1, 0, Color.clear);
        texture.SetPixel(0, 1, Color.white);
        texture.SetPixel(1, 1, Color.black);

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

}