using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum EquipmentSequence
{
    Reforge,
    Strengthen
}

public class SelectEquipmentButton : SelectButton, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private RectTransform mainRect;
    [SerializeField] private float costObjWidth = 100;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    private EquipmentSequence partOfSequence;
    private Equipment representingEquipment;
    private GameObject spawnedToolTip;

    private void Start()
    {
        onPointerClick += DestroyToolTip;
    }

    public void Set(Equipment e, Action a)
    {
        representingEquipment = e;
        Set(e.GetName(), a);
    }

    public void ShowCost(string label, EquipmentSequence inSequence)
    {
        partOfSequence = inSequence;
        labelText.text = label;
        if (partOfSequence == EquipmentSequence.Reforge)
        {
            Utils.SetRight(mainRect, costObjWidth);
            costObj.SetActive(true);
            SetCostText();
        }
    }

    private void SetCostText()
    {
        if (partOfSequence == EquipmentSequence.Reforge)
        {
            costText.text = representingEquipment.GetCostToReforge().ToString();
        }
    }

    private void Update()
    {
        SetText(representingEquipment.GetName());
        SetCostText();
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
