using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class SceneController : MonoBehaviour
{
    public List<OutlineBehaviour> outlines = new List<OutlineBehaviour>();
    public GameObject playerObject;
    public float coneRadius = 1.0f;

    private DialogManager dm;

    // Start is called before the first frame update
    void Start()
    {
        dm = GetComponent<DialogManager>();
    }

    // Update is called once per frame
    void Update()
    {

        int layerMask = 1 << 6;

        RaycastHit hit;
        GameObject lastHit = null;
        Vector3 rayDirection = playerObject.transform.GetChild(0).transform.TransformDirection(Vector3.forward);
        if (Physics.SphereCast(playerObject.transform.position, coneRadius, rayDirection, out hit, 3, layerMask))
        {
            lastHit = hit.transform.gameObject;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.transform.gameObject.tag == "KeyItem")
                {
                    hit.transform.gameObject.SetActive(false);

                    NPCHandler edward = GameObject.Find("Edward").GetComponentInChildren<NPCHandler>();
                    List<string> preDialog = new List<string>{edward.dialogStrings[edward.dialogStrings.Count - 1]};

                    if (hit.transform.gameObject.name == "Key") {
                        preDialog[0] = preDialog[0].Replace(",", "");
                        preDialog[0] = preDialog[0].Replace("Key ", "");
                    } else if (hit.transform.gameObject.name == "Keycard") {
                        preDialog[0] = preDialog[0].Replace("Keycard", "");
                        preDialog[0] = preDialog[0].Replace(",", "");
                    }
                    if (preDialog[0] == "I need: ") {
                        preDialog = new List<string>{"Thanks!"};
                    }
                    edward.dialogStrings = preDialog;
                    edward.dialog = new Dialog(edward.dialog.CharacterName, edward.dialogStrings);
                }
                else if (hit.transform.gameObject.tag == "NPC")
                {
                    dm.SetDialog(hit.transform.gameObject.GetComponent<NPCHandler>().dialog);
                }
            }
        } else {
            if (Input.GetKeyDown(KeyCode.E))
            {
                dm.dialogBox.gameObject.SetActive(false);
                dm.currentDialog = null;
            }
        }

        Debug.DrawLine(playerObject.transform.position, playerObject.transform.position + rayDirection.normalized * 3, Color.red, 0.1f);

        foreach (OutlineBehaviour outline in outlines)
        {
            if (hit.transform != null && hit.transform.gameObject == outline.gameObject) {
                outline.enabled = true;
            } else if (hit.transform == null || hit.transform.gameObject != outline.gameObject || Vector3.Distance(outline.transform.position, playerObject.transform.position) > 3)
            {
                outline.enabled = false;
            }
        }
    }
}
