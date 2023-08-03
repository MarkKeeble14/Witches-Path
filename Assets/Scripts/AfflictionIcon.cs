using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfflictionIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI stacksRemaining;
    [SerializeField] private TextMeshProUGUI durationRemaining;

    private Affliction setTo;

    private void Update()
    {
        SetDurationRemaining(Utils.RoundTo(setTo.RemainingDuration, 0).ToString());
        SetStacksRemaining(Utils.RoundTo(setTo.RemainingActivations, 0).ToString());
    }

    public void SetAffliction(Affliction aff)
    {
        setTo = aff;
        SetTypeText(Utils.SplitOnCapitalLetters(aff.Type.ToString()));
    }

    public void SetSprite(Sprite s)
    {
        image.sprite = s;
    }

    public void SetTypeText(string s)
    {
        typeText.text = s;
    }

    public void SetStacksRemaining(string s)
    {
        stacksRemaining.text = s;
    }

    public void SetDurationRemaining(string s)
    {
        durationRemaining.text = s;
    }
}
