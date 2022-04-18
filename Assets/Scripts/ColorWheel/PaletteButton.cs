using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour
{
    Button button;
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => SetColor());
    }

    public void SetColor()
    {
        VoxelManager.instance.valuePicker.SetColor(button.colors.normalColor, false);
    }

}