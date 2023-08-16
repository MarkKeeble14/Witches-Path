﻿using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectEquipmentButton : SelectButton
{
    private Equipment representingEquipment;

    [SerializeField] private RectTransform mainRect;
    [SerializeField] private float costObjWidth = 100;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    private GameObject spawnedToolTip;

    public void Set(Equipment e, Action a)
    {
        representingEquipment = e;
        Set(e.GetName(), a);
    }

    public void ShowCost(int cost, string label)
    {
        Utils.SetRight(mainRect, costObjWidth);

        costText.text = cost.ToString();
        labelText.text = label;
        costObj.SetActive(true);
    }

    private void Update()
    {
        SetText(representingEquipment.GetName());
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingEquipment, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
