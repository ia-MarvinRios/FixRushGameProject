using UnityEngine;
using UnityEngine.EventSystems;

public class DragRotateInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public System.Action<float> OnDragDelta;

    private bool _isDragging;
    private float _deltaX;

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        _deltaX = eventData.delta.x;
        OnDragDelta?.Invoke(_deltaX);
    }
}