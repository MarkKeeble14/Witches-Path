using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellQueueDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    private int index;
    [SerializeField] private GameObject removeButton;

    public bool CanBeRemoved { get; set; }

    public void Set(string text, Sprite sprite, int index)
    {
        this.text.text = text;
        image.sprite = sprite;
        this.index = index;
    }

    public void RemoveFromQueue()
    {
        CombatManager._Instance.RemoveSpellFromQueue(index);
    }

    private void Update()
    {
        removeButton.SetActive(CanBeRemoved);
    }
}
