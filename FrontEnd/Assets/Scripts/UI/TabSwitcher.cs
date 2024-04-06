using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TabSwitcher : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            FocusNextInputField();
        }
    }
    
    void FocusNextInputField()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i].isFocused)
            {
                int nextIndex = (i + 1) % inputFields.Length;
                
                inputFields[nextIndex].Select();
                inputFields[nextIndex].ActivateInputField();

                break;
            }
        }
    }
}
