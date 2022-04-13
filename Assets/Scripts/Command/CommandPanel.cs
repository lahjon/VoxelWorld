using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandPanel : MonoBehaviour
{
    public ScrollRect scrollRect;
    public CommandListObject commandObject;
    public List<CommandListObject> objs = new List<CommandListObject>();
    CommandListObject _currentHighlight;
    public CommandListObject CurrentHighlight
    {
        get => _currentHighlight;
        set
        {
            if (_currentHighlight != null) _currentHighlight.Highlighted = false;
            _currentHighlight = value;
            _currentHighlight.Highlighted = true;
        }
    }

    public void ClearCommands()
    {
        while (objs.Count > 0)
        {
            Destroy(objs[objs.Count - 1].gameObject);
            objs.RemoveAt(objs.Count - 1);
        }
    }

    public void AddCommand(string command, int index)
    {
        CommandListObject clo = Instantiate(commandObject, scrollRect.content);
        while (index < objs.Count)
        {
            Destroy(objs[objs.Count - 1].gameObject);
            objs.RemoveAt(objs.Count - 1);
        }
        clo.txt.text = command;
        clo.transform.SetAsFirstSibling();
        objs.Add(clo);
        CurrentHighlight = clo;
    }
    public void RedoCommand(int index)
    {
        if (index > 0)
        {
            CurrentHighlight = objs[index - 1];
        }
    }

    public void UndoCommand(int index)
    {
        if (index > 0)
        {
            CurrentHighlight = objs[index - 1];
        }
        else if (index == 0)
        {
            CurrentHighlight.Highlighted = false;
        }
    }
}