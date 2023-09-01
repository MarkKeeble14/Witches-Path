using UnityEngine.EventSystems;
using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class VisualSpellDisplay : SpellDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Spell Display")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private GameObject currentOutOfCombatCooldown;
    [SerializeField] private TextMeshProUGUI currentOutOfCombatCooldownText;
    [SerializeField] private TextMeshProUGUI outOfCombatCooldownText;

    [Header("Spell Dependant")]
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image setColorOf;
    [SerializeField] private TextMeshProUGUI[] coloredTexts;
    private bool isMouseOver;

    private Action onClick;
    private Action onEnter;
    private Action onExit;

    public void AddOnClick(Action a)
    {
        onClick += a;
    }

    public void AddOnEnter(Action a)
    {
        onEnter += a;
    }

    public void AddOnExit(Action a)
    {
        onExit += a;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        onExit?.Invoke();
    }

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTips)
        {
            if (!isMouseOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        base.SpawnToolTip();
    }

    public override void SpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public override void Unset()
    {
        base.Unset();
        onEnter -= SpawnToolTip;
        onExit -= DestroyToolTip;
    }

    public void SetAvailableState(int currentOOCCD)
    {
        // 
        if (currentOOCCD > 0)
        {
            currentOutOfCombatCooldownText.text = "Unavailable\nComplete " + currentOOCCD.ToString() + " Room" + (currentOOCCD > 1 ? "s" : "");
            currentOutOfCombatCooldown.SetActive(true);
            mainCV.interactable = false;
            mainCV.blocksRaycasts = false;
        }
        else
        {
            currentOutOfCombatCooldown.SetActive(false);
            mainCV.interactable = true;
            mainCV.blocksRaycasts = true;
        }
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        SetAvailableState(0);
        outOfCombatCooldownText.text = UIManager._Instance.HighlightKeywords("Out of Combat Cooldown: " + spell.OutOfCombatCooldown + " Room" + (spell.OutOfCombatCooldown > 1 ? "s" : ""));

        onEnter += SpawnToolTip;
        onExit += DestroyToolTip;

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);
        // Set Card Color
        SpellColorInfo colorInfo = UIManager._Instance.GetSpellColor(spell.Color);
        setColorOf.color = colorInfo.Color;
        foreach (TextMeshProUGUI text in coloredTexts)
        {
            text.color = colorInfo.TextColor;
        }

        switch (spell)
        {
            case ActiveSpell activeSpell:
                typeText.text = "Active";

                // Show Detail of Spells
                string[] tokens = spell.GetToolTipText().Split(',');
                string r = "";
                for (int i = 0; i < tokens.Length; i++)
                {
                    r += tokens[i];
                    if (i < tokens.Length - 1)
                    {
                        r += "\n";
                    }
                }
                text.text = r;

                break;
            case PassiveSpell passiveSpell:
                typeText.text = "Passive";
                text.text = spell.GetToolTipText();
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public CanvasGroup GetCanvasGroup()
    {
        return mainCV;
    }
}
