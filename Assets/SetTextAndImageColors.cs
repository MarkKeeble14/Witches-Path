using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UISection
{
    Merchant,
    Librarian,
    Clothier,
    ClothierReforge,
    ClothierStrengthen,
    ClothierBrowse,
    Innkeeper,
    HP,
    Mana,
    Gold,
    Attack,
    Defense,
    Pelts,
    CampfireUpgradeBook,
    CampfireRest,
    CampfireBrewPotions,
    ApproachChest,
    ApproachLecturn
}

public class SetTextAndImageColors : MonoBehaviour
{
    [SerializeField] private UISection uiSection;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image i;

    private void Start()
    {
        UISectionInformation setTo = UIManager._Instance.GetUISectionInformation(uiSection);
        i.color = setTo.Color;
        text.color = setTo.Color;
        i.sprite = setTo.Icon;
        text.text = setTo.Text;
    }
}
