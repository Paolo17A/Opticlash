using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class AdventureController : MonoBehaviour
{
    [field:  SerializeField] private AdventureCore AdventureCore { get; set; }

    [Header("DRAG VARIABLES")]
    [SerializeField] [ReadOnly] private Vector3 initialTouchPoint;
    [SerializeField] [ReadOnly] private Vector3 previousTouchPoint;
    [SerializeField] [ReadOnly] private Vector3 nextTouchPoint;
    [SerializeField] [ReadOnly] private Vector3 draggedEndPoint;
    [SerializeField] [ReadOnly] private Vector3 dummyVector;
    [SerializeField] private float speed;

    [Header("DEBUGGER")]
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    [SerializeField][ReadOnly]private bool isScrolling;

    private void Start()
    {
        GameManager.Instance.SceneController.ActionPass = true;
        GameManager.Instance.PanelActivated = false;
        AdventureCore.InitializeLevels();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(0, Input.mousePosition.y, 0));
            previousTouchPoint = initialTouchPoint;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos2D = new Vector2(mousePos.x, mousePos.y);
            hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
            if (!GameManager.Instance.PanelActivated && !AdventureCore.StageSelectAnimator.GetBool("ShowStageSelect") &&!isScrolling && hit && hit.collider.gameObject.tag == "Level")
            {
                hit.collider.gameObject.GetComponent<LevelSelectController>().ProcessLevel();
            }

            initialTouchPoint = Vector3.zero;
            previousTouchPoint = Vector3.zero;
            nextTouchPoint = Vector3.zero;
            draggedEndPoint = Vector3.zero;
            isScrolling = false;
        }
        if (Input.GetMouseButton(0))
        {
            nextTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(0, Input.mousePosition.y, 0));
            if (nextTouchPoint.y != previousTouchPoint.y && !GameManager.Instance.PanelActivated)
            {
                isScrolling = true;
                draggedEndPoint = previousTouchPoint - nextTouchPoint;
                dummyVector = AdventureCore._virtualCamera.transform.position + new Vector3(0, draggedEndPoint.y, 0);
                AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(dummyVector.y, 0, 95.5f), AdventureCore._virtualCamera.transform.position.z);
            }
        }
        
    }
}
