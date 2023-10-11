using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetSortOrderOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private int sortOrder;
    private int defaultSortOrder;
    [SerializeField] private bool overrideSortOrderByDefault;

    private void Awake()
    {
        defaultSortOrder = canvas.sortingOrder;
        canvas.overrideSorting = overrideSortOrderByDefault;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter: " + name);
        canvas.sortingOrder = sortOrder;
        canvas.overrideSorting = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer Exit: " + name);
        canvas.sortingOrder = defaultSortOrder;
        canvas.overrideSorting = false;
    }
}
