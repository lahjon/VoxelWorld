using TMPro;
using UnityEngine;

public class CommandListObject : MonoBehaviour
{
    public TMP_Text txt;
    bool _highlighted;
    public bool Highlighted
    {
        get => _highlighted;
        set
        {
            _highlighted = value;
            if (_highlighted)
            {
                txt.color = Color.green;
            }
            else
            {
                txt.color = Color.white;
            }
        }
    }
}