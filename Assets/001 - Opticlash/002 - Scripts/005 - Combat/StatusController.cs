using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    [SerializeField] private Animator StatusAnimator;

    public void ResetTrigger()
    {
        StatusAnimator.ResetTrigger("ShowStatus");
    }
}
