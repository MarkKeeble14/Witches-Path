using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SelectStatButton : SelectButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform mainRect;
    [SerializeField] private float costObjWidth = 100;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    private Equipment representingEquipment;

    private GameObject spawnedToolTip;

    private void Start()
    {
        onPointerClick += DestroyToolTip;
    }

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
        costText.text = representingEquipment.GetCostToStrengthen().ToString();
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
