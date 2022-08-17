using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    [SerializeField] private Animator StatusAnimator;
    private void Start()
    {
        Debug.Log("status controoller");
    }
    public void ResetTrigger()
    {
        StatusAnimator.ResetTrigger("ShowStatus");
    }
}
