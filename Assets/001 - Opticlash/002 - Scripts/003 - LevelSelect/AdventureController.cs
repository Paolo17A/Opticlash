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
    [SerializeField][ReadOnly] private GameObject shrunkenLevel;
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    [SerializeField][ReadOnly] private bool isScrolling;
    [SerializeField][ReadOnly] private int normalVector;
    [SerializeField][ReadOnly] private float newCameraPosY;
    private void Start()
    {
        GameManager.Instance.SceneController.ActionPass = true;
        GameManager.Instance.PanelActivated = false;
        AdventureCore.InitializeLevels();
    }

    private void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("using editor");
                initialTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(0, Input.mousePosition.y, 0));
                previousTouchPoint = initialTouchPoint;

                mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos2D = new Vector2(mousePos.x, mousePos.y);
                hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
                if (!GameManager.Instance.PanelActivated && !AdventureCore.StageSelectAnimator.GetBool("ShowStageSelect")  && hit && hit.collider.gameObject.tag == "Level")
                {
                    shrunkenLevel = hit.collider.gameObject;
                    shrunkenLevel.GetComponent<ButtonScaler>().PushButtonDown();
                }

            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (shrunkenLevel != null)
                {
                    shrunkenLevel.GetComponent<ButtonScaler>().PushButtonUp();
                    shrunkenLevel = null;
                }
                mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos2D = new Vector2(mousePos.x, mousePos.y);
                hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
                if (!GameManager.Instance.PanelActivated && !AdventureCore.StageSelectAnimator.GetBool("ShowStageSelect") && !isScrolling && hit && hit.collider.gameObject.tag == "Level")
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
                    AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(dummyVector.y, 0, AdventureCore.MaxYClamp), AdventureCore._virtualCamera.transform.position.z);
                }
            }
        }
        else
        {
            if(Input.touches.Length > 0 && !GameManager.Instance.PanelActivated)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began )
                {
                    Debug.Log("touch began");
                    initialTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(0, Input.GetTouch(0).position.y, 0));
                    previousTouchPoint = initialTouchPoint;
                    newCameraPosY = AdventureCore._virtualCamera.transform.position.y;

                    mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                    mousePos2D = new Vector2(mousePos.x, mousePos.y);
                    hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
                    if (!AdventureCore.StageSelectAnimator.GetBool("ShowStageSelect") && hit && hit.collider.gameObject.tag == "Level")
                    {
                        shrunkenLevel = hit.collider.gameObject;
                        shrunkenLevel.GetComponent<ButtonScaler>().PushButtonDown();
                    }
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    if (shrunkenLevel != null)
                    {
                        shrunkenLevel.GetComponent<ButtonScaler>().PushButtonUp();
                        shrunkenLevel = null;
                    }
                    Debug.Log("touch ended");
                    mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
                    mousePos2D = new Vector2(mousePos.x, mousePos.y);
                    hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
                    if (!GameManager.Instance.PanelActivated && !AdventureCore.StageSelectAnimator.GetBool("ShowStageSelect") && !isScrolling && hit && hit.collider.gameObject.tag == "Level")
                    {
                        hit.collider.gameObject.GetComponent<LevelSelectController>().ProcessLevel();
                    }

                    initialTouchPoint = Vector3.zero;
                    previousTouchPoint = Vector3.zero;
                    nextTouchPoint = Vector3.zero;
                    draggedEndPoint = Vector3.zero;
                    dummyVector = Vector3.zero;
                    isScrolling = false;
                }
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    //Debug.Log("position changed by: " + Input.GetTouch(0).deltaPosition.y);
                    //AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(Input.GetTouch(0).deltaPosition.y, 0, AdventureCore.MaxYClamp), AdventureCore._virtualCamera.transform.position.z);
                    AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(-newCameraPosY, 0, AdventureCore.MaxYClamp), AdventureCore._virtualCamera.transform.position.z);

                    nextTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(0, Input.GetTouch(0).position.y, 0));
                    if (nextTouchPoint.y != previousTouchPoint.y && !GameManager.Instance.PanelActivated)
                    {
                        isScrolling = true;
                        //draggedEndPoint = previousTouchPoint - nextTouchPoint;
                        draggedEndPoint = -Input.GetTouch(0).deltaPosition * Time.deltaTime;
                        newCameraPosY += draggedEndPoint.y;

                        //dummyVector = -(AdventureCore._virtualCamera.transform.position + new Vector3(0, draggedEndPoint.y, 0));
                        AdventureCore._virtualCamera.transform.position = new Vector3(0, Mathf.Clamp(newCameraPosY, 0, AdventureCore.MaxYClamp), AdventureCore._virtualCamera.transform.position.z);
                        //previousTouchPoint = nextTouchPoint;
                        //Debug.Log(AdventureCore._virtualCamera.transform.position);
                    }
                }
            }
        }
        
    }
}
