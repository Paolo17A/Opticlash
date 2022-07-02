using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class BackgroundScroller : MonoBehaviour
{
    //============================================================================
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("PANELS")]
    [field: SerializeField] private GameObject FrontPanel { get; set; }
    [field: SerializeField] private GameObject LastPanel { get; set; }

    [field: SerializeField] private GameObject FrontStage { get; set; }
    [field: SerializeField] private GameObject LastStage { get; set; }

    [field: Header("CONSTANTS")]
    [field: SerializeField] private float windowConstant = 3.92f;
    [field: SerializeField] private float spawnConstant = 14.67f;

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] private GameObject holder;
    [field: SerializeField][field: ReadOnly] private GameObject stageHolder;
    //============================================================================

    void Update()
    {
        FrontPanel.transform.Translate(Vector3.left * Time.deltaTime);
        LastPanel.transform.Translate(Vector3.left * Time.deltaTime);
        if (LastPanel.transform.position.x <= windowConstant)
        {
            FrontPanel.transform.position = new Vector3(spawnConstant, FrontPanel.transform.position.y, FrontPanel.transform.position.z);
            holder = FrontPanel;
            FrontPanel = LastPanel;
            LastPanel = holder;
            holder = null;
        }

        if(CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            FrontStage.transform.Translate(Vector3.left * Time.deltaTime);
            LastStage.transform.Translate(Vector3.left * Time.deltaTime);
            if (LastStage.transform.position.x <= windowConstant)
            {
                FrontStage.transform.position = new Vector3(spawnConstant, FrontStage.transform.position.y, FrontStage.transform.position.z);
                stageHolder = FrontStage;
                FrontStage = LastStage;
                LastStage = stageHolder;
                stageHolder = null;
            }
        }
    }
}
