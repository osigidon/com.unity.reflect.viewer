using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CivilFX.UI2
{
    public class UIDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private Vector2 offsetToMouse;
        public void OnDrag(PointerEventData eventData)
        {
            gameObject.transform.position = eventData.position - offsetToMouse;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 rectPos = gameObject.transform.position;
            offsetToMouse = eventData.position - rectPos;
        }
    }
}