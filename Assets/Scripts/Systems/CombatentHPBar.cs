using UnityEngine;
using TMPro;

public class CombatentHPBar : MonoBehaviour
{
    [SerializeField] private CombatentHPBarSegment segmentPrefab;

    [SerializeField] private Transform parentSegmentsTo;

    [SerializeField] private TextMeshProUGUI hpText;

    private CombatentHPBarSegment[] hPBarSegments;

    [SerializeField] private TextMeshProUGUI wardText;
    [SerializeField] private GameObject[] wardDisplay;
    private int currentWard;
    private int maxHealth;
    private int currentHealth;

    private int damageFromPoison;
    private int damageFromBurn;
    private int damageFromBlight;

    public void SetDamageFromPoison(int stacks)
    {
        damageFromPoison = stacks;
        ShowAfflictionStacks();
    }

    public void SetDamageFromBurn(int stacks)
    {
        damageFromBurn = stacks;
        ShowAfflictionStacks();
    }

    public void SetDamageFromBlight(int stacks)
    {
        damageFromBlight = stacks;
        ShowAfflictionStacks();
    }

    private void ShowAfflictionStacks()
    {
        int poisonThreshold = currentHealth - damageFromPoison;
        int burnThreshold = currentHealth - damageFromPoison - damageFromBurn;
        int blightThreshold = currentHealth - damageFromPoison - damageFromBurn - damageFromBlight;
        for (int i = currentHealth; i > poisonThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            segment.SetColor(Color.green);
        }

        for (int i = poisonThreshold; i > burnThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            segment.SetColor(Color.magenta);
        }

        for (int i = blightThreshold; i > blightThreshold && i > 0; i--)
        {
            CombatentHPBarSegment segment = hPBarSegments[i - 1];
            segment.SetColor(Color.cyan);
        }
    }

    public void Set(int currentHealth, int maxHealth)
    {
        hPBarSegments = new CombatentHPBarSegment[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            CombatentHPBarSegment spawned = Instantiate(segmentPrefab, parentSegmentsTo);
            hPBarSegments[i] = spawned;
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
        hpText.text = currentHealth + " / " + maxHealth;
    }

    public void SetCurrentHP(int newCurrentHealth)
    {
        for (int i = 0; i < hPBarSegments.Length; i++)
        {
            CombatentHPBarSegment segment = hPBarSegments[i];
            // Hide segments that are indexed higher than the current HP Value
            if (i >= newCurrentHealth)
            {
                segment.SetAlpha(0);
            }
            else
            {
                segment.SetAlpha(1);
            }
        }
        currentHealth = newCurrentHealth;
        hpText.text = currentHealth + " / " + maxHealth;
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
    }

    public void SetText(string text)
    {
        this.hpText.text = text;
    }
}
