using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    public void DeactivatePanel()
    {
        GameManager.Instance.PanelActivated = false;
    }
}
