using UnityEngine;
using TMPro;

public class SelectStatButton : SelectButton
{
    [SerializeField] private RectTransform mainRect;
    [SerializeField] private float costObjWidth = 100;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    private Equipment representingEquipment;

    private GameObject spawnedToolTip;

    public void ReupToolTip()
    {
        DestroyToolTip();
        SpawnToolTip();
    }

    public void ShowCost(Equipment representingEquipment, string label)
    {
        Utils.SetRight(mainRect, costObjWidth);

        this.representingEquipment = representingEquipment;

        labelText.text = label;
        costObj.SetActive(true);
    }

    private void Update()
    {
        costText.text = representingEquipment.GetCostToBoost().ToString();
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
