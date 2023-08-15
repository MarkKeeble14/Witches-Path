using UnityEngine;
using TMPro;

public class SelectStatButton : SelectButton
{
    [SerializeField] private RectTransform mainRect;
    [SerializeField] private float costObjWidth = 100;

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject costObj;

    public void ShowCost(int cost, string label)
    {
        Utils.SetRight(mainRect, costObjWidth);

        costText.text = cost.ToString();
        labelText.text = label;
        costObj.SetActive(true);
    }
}
