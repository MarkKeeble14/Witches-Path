using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveButton : MonoBehaviour
{
    public void ResolveCurrentEvent()
    {
        GameManager._Instance.ResolveCurrentEvent();
    }
}
