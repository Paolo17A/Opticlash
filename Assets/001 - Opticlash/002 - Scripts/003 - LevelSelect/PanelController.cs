using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    private void Awake()
    {
        
    }
    public void DeactivatePanel()
    {
        GameManager.Instance.PanelActivated = false;
    }
}
