using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SwapBookButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image icon;

    private Action onClick;
    private Action onEnter;
    private Action onExit;

    public void Set(BookLabel label, Action onClick)
    {
        text.text = label.ToString();
        this.onClick += onClick;

        // Spawn ToolTip
        GameObject spawnedToolTip = null;
        Book book = GameManager._Instance.GetBookOfType(label);

        // Add Callbacks
        onEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(book, transform);
        };

        onExit += delegate
        {
            Destroy(spawnedToolTip);
        };
    }

    public void OnClick()
    {
        onClick?.Invoke();
    }

    public void OnEnter()
    {
        onEnter?.Invoke();
    }

    public void OnExit()
    {
        onExit?.Invoke();
    }
}