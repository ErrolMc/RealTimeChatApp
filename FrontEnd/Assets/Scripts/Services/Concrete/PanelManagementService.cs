using System.Collections;
using System.Collections.Generic;
using ChatApp.Enums;
using ChatApp.UI;
using UnityEngine;
using Zenject;

namespace ChatApp.Services.Concrete
{
    public class PanelManagementService : MonoBehaviour, IPanelManagementService, IInitializable
    {
        [SerializeField] private Panel[] panels;
        [SerializeField] private PanelID initialPanel;

        public PanelID CurrentPanel { get; set; } = PanelID.None;
        
        public void ShowPanel(PanelID panelID)
        {
            Debug.LogError("ShowPanel " + panelID);
            
            if (panelID == CurrentPanel)
                return;
            
            foreach (Panel panel in panels)
            {
                if (panel == null)
                {
                    Debug.LogError("Panel " + panelID + "is null");
                    continue;
                }

                
                if (panel.PanelID == panelID)
                    panel.OnShow();
                else
                    panel.OnHide();
            }
        }

        public void Initialize()
        {
            ShowPanel(initialPanel);
        }
    }   
}
