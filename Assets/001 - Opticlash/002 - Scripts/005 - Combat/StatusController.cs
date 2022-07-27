using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    [SerializeField] private Animator StatusAnimator;
    private void Start()
    {
        
    }
    public void ResetTrigger()
    {
        StatusAnimator.ResetTrigger("ShowStatus");
    }
}
