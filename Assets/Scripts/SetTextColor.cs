using UnityEngine;
using TMPro;

public class SetTextColor : MonoBehaviour
{
    [SerializeField] private UISection uiSection;

    [SerializeField] private TextMeshProUGUI text;
    private void Start()
    {
        UISectionInformation setTo = UIManager._Instance.GetUISectionInformation(uiSection);
        text.color = setTo.Color;
    }
}