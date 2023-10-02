using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CombatentHPBar : MonoBehaviour
{
    [SerializeField] private CombatentHPBarSegment segmentPrefab;

    [Header("References")]
    [SerializeField] private Transform parentSegmentsTo;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI wardText;
    [SerializeField] private GameObject[] wardDisplay;
    private CombatentHPBarSegment[] hPBarSegments;
    private int currentWard;
    private int maxHealth;
    private int currentHealth;

    [Header("Animations")]
    [SerializeField] private float betweenSegmentDelay = .02f;
    [SerializeField] private float delay = .5f;
    [SerializeField] private Color changingColor;
    private List<CombatentHPBarSegment> changingSegments = new List<CombatentHPBarSegment>();
    private float canAnimateDownDelayTimer;
    private float betweenSegmentDelayTimer;
    private int shownCurrentHealth;

    public bool Empty => shownCurrentHealth == 0;

    private int damageFromPoison;
    private int damageFromBurn;
    private int damageFromBlight;

    public void Set(int currentHealth, int maxHealth)
    {
        hPBarSegments = new CombatentHPBarSegment[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            CombatentHPBarSegment spawned = Instantiate(segmentPrefab, parentSegmentsTo);
            hPBarSegments[i] = spawned;
            spawned.SetPosition(i);
            spawned.name += "<" + i + ">";
            spawned.SetColor(Color.red);

            // Hide segments that are indexed higher than the current HP Value
            if (i > currentHealth)
            {
                spawned.SetAlpha(0);
            }
            else
            {
                spawned.SetAlpha(1);
            }
        }
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;
        shownCurrentHealth = currentHealth;
        hpText.text = currentHealth + " / " + maxHealth;
    }

    public void SetCurrentHP(int newCurrentHealth)
    {
        // Any Segments that need to be changed will have these characteristics
        for (int i = shownCurrentHealth; i != newCurrentHealth;)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            if (!changingSegments.Contains(segment))
            {
                changingSegments.Add(segment);
                segment.SetColor(changingColor);
                segment.SetAlpha(1);
            }

            if (i < newCurrentHealth)
            {
                i++;
            }
            else
            {
                i--;
            }
        }

        // Set Timers
        canAnimateDownDelayTimer = delay;
        betweenSegmentDelayTimer = 0;

        // Set
        currentHealth = newCurrentHealth;
        hpText.text = currentHealth + " / " + maxHealth;
    }

    private void Update()
    {
        if (canAnimateDownDelayTimer > 0)
        {
            // Reduce Timer
            canAnimateDownDelayTimer -= Time.deltaTime;
        }
        else
        {
            // First Check if there are segments to change
            if (changingSegments.Count > 0)
            {
                // Sort by Position
                changingSegments.Sort((x, y) => x.Position.CompareTo(y.Position));

                if (betweenSegmentDelayTimer > 0)
                {
                    // Reduce Timer
                    betweenSegmentDelayTimer -= Time.deltaTime;
                }
                else
                {
                    // Changing Segment
                    CombatentHPBarSegment segment;

                    // Set Characteristics
                    if (shownCurrentHealth < currentHealth)
                    {
                        segment = changingSegments[0];
                        segment.SetAlpha(1);
                    }
                    else
                    {
                        segment = changingSegments[changingSegments.Count - 1];
                        shownCurrentHealth--;
                        segment.SetAlpha(0);
                    }
                    segment.SetColor(Color.red);

                    // Reset Timer
                    betweenSegmentDelayTimer = betweenSegmentDelay;

                    changingSegments.Remove(segment);
                }
            }
        }

        ShowAfflictionStacks();
    }

    public void SetWard(int wardAmount)
    {
        if (currentWard <= 0 && wardAmount > 0)
        {
            // Gaining Ward from nothing
            foreach (GameObject obj in wardDisplay)
            {
                obj.SetActive(true);
            }
        }
        else if (currentWard > 0 && wardAmount <= 0)
        {
            // Losing remaining Ward
            foreach (GameObject obj in wardDisplay)
            {
                obj.SetActive(false);
            }
        }
        currentWard = wardAmount;
        wardText.text = currentWard.ToString();
    }

    public void Clear()
    {
        foreach (CombatentHPBarSegment segment in hPBarSegments)
        {
            Destroy(segment.gameObject);
        }
        damageFromBlight = 0;
        damageFromBurn = 0;
        damageFromPoison = 0;
    }

    public void SetText(string text)
    {
        this.hpText.text = text;
    }

    private void ShowAfflictionStacks()
    {
        int poisonThreshold = currentHealth - damageFromPoison;
        int burnThreshold = currentHealth - damageFromPoison - damageFromBurn;
        int blightThreshold = currentHealth - damageFromPoison - damageFromBurn - damageFromBlight;
        for (int i = currentHealth; i > poisonThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            // Green
            segment.SetColor(Utils.ParseHexToColor("#90EE90"));
        }

        for (int i = poisonThreshold; i > burnThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            // Orange
            segment.SetColor(Utils.ParseHexToColor("#FFA500"));
        }

        for (int i = burnThreshold; i > blightThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            segment.SetColor(Color.yellow);
        }
    }

    public void SetDamageFromPoison(int stacks)
    {
        damageFromPoison = stacks;
    }

    public void SetDamageFromBurn(int stacks)
    {
        damageFromBurn = stacks;
    }

    public void SetDamageFromBlight(int stacks)
    {
        damageFromBlight = stacks;
    }
}
