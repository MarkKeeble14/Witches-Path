using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableDebugUpdater : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }
}
