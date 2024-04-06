using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ChatApp.UI
{
    // login
    public partial class LoginPanel : Panel
    {
        [Header("Login")] 
        [SerializeField] private GameObject loginPanel;

        [SerializeField] private Button loginButton;
        [SerializeField] private TMP_InputField loginUsernameInputField;
        [SerializeField] private TMP_InputField loginPasswordInputField;

        public void OnClick_Login()
        {
            
        }

        public void OnClick_Register_FromLogin()
        {
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
        }
        
        public override void OnShow()
        {
            ResetFields();
            base.OnShow();
        }

        public override void OnHide()
        {
            ResetFields();
            base.OnHide();
        }

        public void OnValueChanged_LoginUsernameField(string text)
        {
            
        }

        public void OnValueChanged_LoginPasswordField(string text)
        {
            
        }

        private void ResetFields()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            
            loginUsernameInputField.SetTextWithoutNotify("");
            loginPasswordInputField.SetTextWithoutNotify("");
            registerUsernameInputField.SetTextWithoutNotify("");
            registerPasswordInputField.SetTextWithoutNotify("");
            registerConfirmPasswordInputField.SetTextWithoutNotify("");

            loginButton.enabled = false;
            registerButton_RegisterPanel.enabled = false;
        }
    }   
}
