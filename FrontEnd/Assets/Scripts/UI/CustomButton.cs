using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChatApp.UI
{
     [RequireComponent(typeof(Image))]
     public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
     {
         [SerializeField] public Color normalColor = Color.white;
         [SerializeField] public Color hoverColor = Color.red;
         [SerializeField] public Color pressedColor = Color.blue;
     
         [Space(30)]
         [SerializeField] public UnityEvent OnClick;
     
         private Image _buttonImage;
         private bool _isHovered;
         private bool _isPressed;
     
         void OnEnable()
         {
             _buttonImage = GetComponent<Image>();
             _buttonImage.color = normalColor;
         }
     
         public void OnPointerEnter(PointerEventData eventData)
         {
             _isHovered = true;
             if (!_isPressed)
             {
                 _buttonImage.color = hoverColor;
             }
         }
     
         public void OnPointerExit(PointerEventData eventData)
         {
             _isHovered = false;
             if (!_isPressed)
             {
                 _buttonImage.color = normalColor;
             }
         }
         
         public void OnPointerDown(PointerEventData eventData)
         {
             _isPressed = true;
             _buttonImage.color = pressedColor;
         }
     
         public void OnPointerUp(PointerEventData eventData)
         {
             _isPressed = false;
             _buttonImage.color = _isHovered ? hoverColor : normalColor;
             
             if (_isHovered)
                 OnClick?.Invoke();
         }
     }   
}
