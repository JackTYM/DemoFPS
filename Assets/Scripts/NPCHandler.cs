using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHandler : MonoBehaviour
{

    public Dialog dialog;
    public List<String> dialogStrings;
    public String characterName;

    // Start is called before the first frame update
    void Start()
    {
        dialog = new Dialog(characterName, dialogStrings);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
