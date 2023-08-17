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
        hpText.text = currentHealth + " / " + maxHealth;
    }

    public void SetCurrentHP(int newCurrentHP)
    {
        for (int i = 0; i < hPBarSegments.Length; i++)
        {
            CombatentHPBarSegment segment = hPBarSegments[i];
            // Hide segments that are indexed higher than the current HP Value
            if (i >= newCurrentHP)
            {
                segment.SetAlpha(0);
            }
            else
            {
                segment.SetAlpha(1);
            }
        }
        hpText.text = newCurrentHP + " / " + maxHealth;
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
