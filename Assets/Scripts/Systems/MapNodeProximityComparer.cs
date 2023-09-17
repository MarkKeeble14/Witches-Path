using System.Collections.Generic;
using UnityEngine;

class MapNodeProximityComparer : IComparer<MapNodeUI>
{
    private RectTransform proximityTo;
    public MapNodeProximityComparer(MapNodeUI proximityTo)
    {
        this.proximityTo = proximityTo.transform as RectTransform;
    }

    public int Compare(MapNodeUI xNode, MapNodeUI yNode)
    {
        RectTransform x = xNode.transform as RectTransform;
        RectTransform y = yNode.transform as RectTransform;

        float xDist = Vector3.Distance(x.anchoredPosition, proximityTo.anchoredPosition);
        float yDist = Vector3.Distance(y.anchoredPosition, proximityTo.anchoredPosition);

        if (xDist == yDist)
        {
            return 0;
        }

        return xDist.CompareTo(yDist);
    }
}
