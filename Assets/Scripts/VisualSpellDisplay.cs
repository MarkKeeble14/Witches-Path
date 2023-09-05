using UnityEngine.EventSystems;
using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class VisualSpellDisplay : SpellDisplay
{
    [Header("Visual Spell Display")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI activeTypeText;

    [Header("Spell Dependant")]
    [SerializeField] private Image rarityImage;
    private bool isForUpgrade;

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

        if (!isMouseOver)
        {
            yield break;
        }

        SpawnToolTipFunc();
    }

    protected override void SpawnToolTipFunc()
    {
        if (isForUpgrade)
        {
            Spell upgradedSpell = Spell.GetSpellOfType(Spell.Label);

            if (upgradedSpell.CanUpgrade)
            {
                upgradedSpell.Upgrade(Sign.Positive);
                spawnedToolTip = UIManager._Instance.SpawnComparisonToolTips(
                    new ToolTippableComparisonData[]
                        {
                        new ToolTippableComparisonData("Current: ", Spell),
                        new ToolTippableComparisonData("Upgraded: ", upgradedSpell)
                        },
                    transform);
            }
            else
            {
                base.SpawnToolTipFunc();
            }
        }
        else
        {
            base.SpawnToolTipFunc();
        }
    }

    public override void CallSpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public void SetIsForUpgrade(bool b)
    {
        isForUpgrade = b;
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);


        switch (spell)
        {
            case ActiveSpell activeSpell:
                typeText.text = "Active";
                activeTypeText.gameObject.SetActive(true);
                activeTypeText.text = activeSpell.ActiveSpellType.ToString();

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
                activeTypeText.gameObject.SetActive(false);
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
