using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ChatApp.UI
{
    public class TabSwitcher : MonoBehaviour
    {
        public TMP_InputField[] inputFields;
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FocusNextInputField();
            }
        }
    
        private void FocusNextInputField()
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
}
