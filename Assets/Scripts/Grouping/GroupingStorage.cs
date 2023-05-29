using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GroupingStorage", fileName = "GroupingStorageSO")]
public class GroupingStorage : ScriptableObject
{
    [SerializeField] private List<Grouping> grouping = new List<Grouping>();

    public List<Grouping> GetGroupingList => grouping;
    public Grouping GetGrouping(GroupingType groupingType)
    {
        return grouping.Find((_group) => _group.Type == groupingType);
    }
}

[Serializable] 
public class Grouping
{
    [SerializeField] private GroupingType groupingType = default;
    [SerializeField] private Sprite sprite = default;
    [SerializeField] private Color color = default;
    [SerializeField] private Color colorIcon = default;
    [SerializeField] private Color colorBase = default;
    [SerializeField] private Color colorStickmans = default;

    public GroupingType Type => groupingType;
    public Color Color => color;
    public Sprite Sprite => sprite;
    public Color UseColorToIcon => colorIcon;
    public Color ColorBase => colorBase;
    public Color ColorStickmans => colorStickmans;
}
