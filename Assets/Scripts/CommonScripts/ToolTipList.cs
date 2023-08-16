using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipList : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup listPrefab;
    [SerializeField] private RectTransform rect;

    private List<VerticalLayoutGroup> spawnedVLayouts = new List<VerticalLayoutGroup>();

    [SerializeField] private HorizontalLayoutGroup hLayout;

    public VerticalLayoutGroup SpawnList()
    {
        VerticalLayoutGroup spawned = Instantiate(listPrefab, transform);
        spawnedVLayouts.Add(spawned);
        return spawned;
    }

    public RectTransform GetRect()
    {
        return rect;
    }

    public VerticalLayoutGroup GetVerticalLayoutGroup(int index)
    {
        return spawnedVLayouts[index];
    }

    public HorizontalLayoutGroup GetHorizontalLayoutGroup()
    {
        return hLayout;
    }
}