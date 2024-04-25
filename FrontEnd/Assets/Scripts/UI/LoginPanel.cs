using System.Collections;
using System.Collections.Generic;
using ChatApp.Enums;
using ChatApp.Services;
using ChatApp.Shared.Tables;
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
        [SerializeField] private GameObject loadingIcon;
        
        [Header("Login")] 
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton_LoginPanel;
        [SerializeField] private TMP_InputField loginUsernameInputField;
        [SerializeField] private TMP_InputField loginPasswordInputField;

        [Inject] private IAuthenticationService _authenticationService;
        [Inject] private IPanelManagementService _panelManagementService;
        [Inject] private INotificationService _notificationService;
        [Inject] private IFriendService _friendService;

        private bool talkingToServer = false;

        public async void OnClick_Login()
        {
            if (talkingToServer) return;
            talkingToServer = true;
            
            loadingIcon.gameObject.SetActive(true);
            registerButton_LoginPanel.enabled = false;
            
            string username = loginUsernameInputField.text;
            string password = loginPasswordInputField.text;

            (bool, string, User) result = await _authenticationService.TryLogin(username, password);
            responseText.text = result.Item2;

            if (result.Item1)
            {
                User user = result.Item3;
                (bool, string) signalRConnectionResp= await _notificationService.ConnectToSignalR(user);
                if (signalRConnectionResp.Item1)
                {
                    _authenticationService.CurrentUser = user;
                    _authenticationService.IsLoggedIn = true;
                    
                    await _friendService.GetFriendRequests();
                    
                    _panelManagementService.ShowPanel(PanelID.Chat);
                }
                else
                {
                    responseText.text = signalRConnectionResp.Item2;
                }
            }

            loadingIcon.gameObject.SetActive(false);
            registerButton_LoginPanel.enabled = true;
            talkingToServer = false;
        }

        public void OnClick_Register_FromLogin()
        {
            if (talkingToServer)
                return;
            
            ResetFields(true);
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
        }
        
        public override void OnShow()
        {
            _authenticationService.IsLoggedIn = false;
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
            loadingIcon.gameObject.SetActive(false);
        }
    }   
}
