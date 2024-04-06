using System.Collections;
using System.Collections.Generic;
using ChatApp.Enums;
using ChatApp.Services;
using ChatApp.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace ChatApp.UI
{
    // login
    public partial class LoginPanel : Panel
    {
        [Header("Shared")] 
        [SerializeField] private TextMeshProUGUI responseText;
        
        [Header("Login")] 
        [SerializeField] private GameObject loginPanel;

        [SerializeField] private Button loginButton;
        [SerializeField] private TMP_InputField loginUsernameInputField;
        [SerializeField] private TMP_InputField loginPasswordInputField;

        [Inject] private IAuthenticationService _authenticationService;
        [Inject] private IPanelManagementService _panelManagementService;

        private bool talkingToServer = false;

        public async void OnClick_Login()
        {
            if (talkingToServer) return;
            talkingToServer = true;
            
            string username = loginUsernameInputField.text;
            string password = loginPasswordInputField.text;

            (bool, string) result = await _authenticationService.TryLogin(username, password);
            responseText.text = result.Item2;

            if (result.Item1)
            {
                _panelManagementService.ShowPanel(PanelID.Chat);
            }

            talkingToServer = false;
        }

        public void OnClick_Register_FromLogin()
        {
            ResetFields(true);
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
            loginButton.enabled = CanSubmit_Login();
        }

        public void OnValueChanged_LoginPasswordField(string text)
        {
            loginButton.enabled = CanSubmit_Login();
        }
        
        private bool CanSubmit_Login()
        {
            bool stuffInUsername = !string.IsNullOrEmpty(loginUsernameInputField.text);
            bool stuffInPassword = !string.IsNullOrEmpty(loginPasswordInputField.text);

            return stuffInUsername && stuffInPassword;
        }

        private void ResetFields(bool keepPanel = false)
        {
            if (!keepPanel)
            {
                loginPanel.SetActive(true);
                registerPanel.SetActive(false);   
            }

            responseText.SetText("");
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
