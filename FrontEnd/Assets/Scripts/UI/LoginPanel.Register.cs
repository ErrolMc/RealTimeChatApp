using System.Collections;
using System.Collections.Generic;
using ChatApp.Utils;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private Button backButton_RegisterPanel;
        [SerializeField] private Button registerButton_RegisterPanel;

        public void OnClick_Back_FromRegister()
        {
            ResetFields(true);
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }

        public async void OnClick_Register_FromRegister()
        {
            if (talkingToServer) return;
            talkingToServer = true;
            
            string username = registerUsernameInputField.text;
            string password = registerPasswordInputField.text;
            string passwordConfirm = registerConfirmPasswordInputField.text;
            
            if (password.Equals(passwordConfirm) == false)
            {
                responseText.text = "Passwords dont match!";
                return;
            }
            
            backButton_RegisterPanel.enabled = false;
            loadingIcon.gameObject.SetActive(true);

            (bool, string) result = await _authenticationService.TryRegister(username, password);
            
            if (result.Item1)
            {
                OnClick_Back_FromRegister();
            }
            
            responseText.text = result.Item2;
            
            backButton_RegisterPanel.enabled = true;
            loadingIcon.gameObject.SetActive(false);
            
            talkingToServer = false;
        }
        
        public void OnValueChanged_RegisterUsernameField(string text)
        {
            registerButton_RegisterPanel.enabled = CanSubmit_Register();
        }
        
        public void OnValueChanged_RegisterPasswordField(string text)
        {
            registerButton_RegisterPanel.enabled = CanSubmit_Register();
        }
        
        public void OnValueChanged_RegisterConfirmPasswordField(string text)
        {
            registerButton_RegisterPanel.enabled = CanSubmit_Register();
        }

        private bool CanSubmit_Register()
        {
            bool stuffInUsername = !string.IsNullOrEmpty(registerUsernameInputField.text);
            bool stuffInPassword = !string.IsNullOrEmpty(registerPasswordInputField.text);
            bool stuffInConfirmPassword = !string.IsNullOrEmpty(registerConfirmPasswordInputField.text);

            return stuffInUsername && stuffInPassword && stuffInConfirmPassword;
        }
    }
}

