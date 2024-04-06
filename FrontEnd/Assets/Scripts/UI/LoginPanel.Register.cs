using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatApp.UI
{
    // register
    public partial class LoginPanel : Panel
    {
        [Header("Register")] 
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField registerUsernameInputField;
        [SerializeField] private TMP_InputField registerPasswordInputField;
        [SerializeField] private TMP_InputField registerConfirmPasswordInputField;
        [Space(10)]
        [SerializeField] private Button registerButton_RegisterPanel;

        public void OnClick_Back_FromRegister()
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }

        public void OnClick_Register_FromRegister()
        {
            
        }
        
        public void OnValueChanged_RegisterUsernameField(string text)
        {
            
        }
        
        public void OnValueChanged_RegisterPasswordField(string text)
        {
            
        }
        
        public void OnValueChanged_RegisterConfirmPasswordField(string text)
        {
            
        }
    }
}

