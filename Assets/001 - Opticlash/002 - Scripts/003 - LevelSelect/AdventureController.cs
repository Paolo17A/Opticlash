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
    [SerializeField] private float speed;

    [Header("DEBUGGER")]
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;

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
            initialTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.y));
            previousTouchPoint = initialTouchPoint;
        }
        else if (Input.GetMouseButton(0))
        {
            nextTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if (nextTouchPoint != previousTouchPoint && !GameManager.Instance.PanelActivated)
            {
                draggedEndPoint = previousTouchPoint - nextTouchPoint;

                AdventureCore._virtualCamera.transform.position += new Vector3(0, draggedEndPoint.y, 0) * speed * Time.deltaTime;
                AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(AdventureCore._virtualCamera.transform.position.y, 0, 19f), AdventureCore._virtualCamera.transform.position.z);

                previousTouchPoint = nextTouchPoint;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            initialTouchPoint = new Vector3(0, 0, 0);
            previousTouchPoint = new Vector3(0, 0, 0);
            nextTouchPoint = new Vector3(0, 0, 0);
            draggedEndPoint = new Vector3(0, 0, 0);

            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos2D = new Vector2(mousePos.x, mousePos.y);
            hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
            if (!GameManager.Instance.PanelActivated && hit && hit.collider.gameObject.tag == "Level")
            {
                hit.collider.gameObject.GetComponent<LevelSelectController>().ProcessLevel();
            }
        }
    }
}
