using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour
{
    public Button[] paletts;
    public int index;
    int paletteCount;

    void Awake()
    {
        paletteCount = 6;
        paletts = new Button[paletteCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            paletts[i] = transform.GetChild(i).GetComponent<Button>();
        }
    }

    public void UpdateColor(Color color)
    {
        if (index >= paletteCount)
        {
            index = 0;
        }
        ColorBlock colors = paletts[index].colors;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.normalColor = color;
        paletts[index].colors = colors;
        index++;
        Debug.Log(index);
    } 

}