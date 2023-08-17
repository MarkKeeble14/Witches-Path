using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellQueueDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;

    private Spell representingSpell;

    private GameObject spawnedToolTip;

    public void Set(Spell spell, Sprite sprite)
    {
        representingSpell = spell;
        text.text = spell.Name;
        image.sprite = sprite;
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingSpell, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
