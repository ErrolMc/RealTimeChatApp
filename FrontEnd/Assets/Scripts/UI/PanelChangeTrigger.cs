using System.Collections;
using System.Collections.Generic;
using ChatApp.Enums;
using ChatApp.Services;
using UnityEngine;
using Zenject;

namespace ChatApp.UI
{
    public class PanelChangeTrigger : MonoBehaviour
    {
        [SerializeField] private PanelID destinationPanel;
        
        [Inject] private IPanelManagementService _panelManagementService;

        public void OnClick_GotoPanel()
        {
            _panelManagementService.ShowPanel(destinationPanel);
        }
    }
}

