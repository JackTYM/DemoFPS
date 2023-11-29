using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogBox;
    public Dialog currentDialog;
    public int dialogIndex = 0;

    public void SetDialog(Dialog dialog) {
        if (currentDialog == dialog) {
            dialogIndex++;

            if (dialogIndex >= currentDialog.Lines.Count) {
                dialogBox.gameObject.SetActive(false);
                currentDialog = null;
                return;
            }
        } else {
            currentDialog = dialog;
            dialogIndex = 0;
        }

        dialogBox.SetActive(true);
        dialogBox.transform.GetChild(1).GetComponent<TMP_Text>().text = currentDialog.CharacterName;
        dialogBox.transform.GetChild(2).GetComponent<TMP_Text>().text = currentDialog.Lines[dialogIndex];
    }
}

public class Dialog
{
    public string CharacterName { get; set; }
    public List<string> Lines { get; set; }

    public Dialog(string characterName, List<string> lines)
    {
        CharacterName = characterName;
        Lines = lines;
    }

    public Dialog(string characterName, string line)
    {
        CharacterName = characterName;
        Lines = new List<string> { line };
    }
}
