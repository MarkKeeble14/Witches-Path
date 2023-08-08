using System;
using UnityEngine.UI;

public class SelectEquipmentButton : SelectButton
{
    private Equipment representingEquipment;

    public void Set(Equipment e, Action a)
    {
        representingEquipment = e;
        Set(e.Name, a);
    }

    private void Update()
    {
        SetText(representingEquipment.Name);
    }
}
