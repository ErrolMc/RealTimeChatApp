using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatApp.UI
{
    public class KeyButtonPresser : MonoBehaviour
    {
        [SerializeField] private KeyCode key;
        [SerializeField] private Button button;
    
        void Update()
        {
            if (Input.GetKeyDown(key) && button.gameObject.activeInHierarchy)
            {
                button.onClick.Invoke();
            }
        }
    }
}

