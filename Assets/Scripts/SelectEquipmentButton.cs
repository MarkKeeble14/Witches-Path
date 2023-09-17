using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SelectEquipmentButton : SelectButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform mainRect;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    private Equipment representingEquipment;
    private GameObject spawnedToolTip;

    private void Start()
    {
        onPointerClick += DestroyToolTip;
    }

    public void Set(Equipment e, Action a)
    {
        representingEquipment = e;
        Set(e.name, a);
    }

    public void ShowCost(string label)
    {
        labelText.text = label;
    }

    private void Update()
    {
        SetText(representingEquipment.name);
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingEquipment, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
