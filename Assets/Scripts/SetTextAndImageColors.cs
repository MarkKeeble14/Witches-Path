using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UISection
{
    HP,
    Mana,
    Gold,
    Attack,
    Defense,
    Pelts,
    CampfireRest,
    CampfireUpgradeBook,
    CampfireUpgradeEquipment,
    CampfireBrewPotions,
}

public class SetTextAndImageColors : MonoBehaviour
{
    [SerializeField] private UISection uiSection;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image i;

    private void Start()
    {
        UISectionInformation setTo = UIManager._Instance.GetUISectionInformation(uiSection);
        if (text)
        {
            text.color = setTo.Color;
            text.text = setTo.Text;
        }

        if (i)
        {
            i.color = setTo.Color;
            i.sprite = setTo.Icon;
        }
    }
}
