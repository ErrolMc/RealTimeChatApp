using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChatApp.Enums;

namespace ChatApp.UI
{
    public abstract class Panel : MonoBehaviour
    {
        [SerializeField] private PanelID panelID;

        public PanelID PanelID => panelID;
        
        public virtual void OnShow()
        {
            gameObject.SetActive(true);
            Debug.LogWarning($"PanelManagement: Showing Panel {panelID.ToString()}");
        }

        public virtual void OnHide()
        {
            gameObject.SetActive(false);   
        }
    }
}

