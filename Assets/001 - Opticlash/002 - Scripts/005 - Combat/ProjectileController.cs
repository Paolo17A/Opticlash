using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private CombatCore combatCore;

    [Header("ROUTE")]
    [SerializeField] private Transform designatedRoute;
    [SerializeField] private Transform endPoint;
    [SerializeField][ReadOnly] private float tParam;
    [SerializeField][ReadOnly] private Vector2 objectPosition;
    [SerializeField] private float speedModifier;
    [SerializeField][ReadOnly] private bool coroutineAllowed;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private Vector2 p0;
    [SerializeField][ReadOnly] private Vector2 p1;
    [SerializeField][ReadOnly] private Vector2 p2;
    [SerializeField][ReadOnly] private Vector2 p3;

    // Start is called before the first frame update
    private void Start()
    {
        tParam = 0f;
        coroutineAllowed = true;
    }

    // Update is called once per frame
    /*void Update()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(GoByTheRoute());
        }
    }*/

    public IEnumerator GoByTheRoute()
    {
        combatCore.SpawnedPlayer.ProjectileCoroutineAllowed = false;
        coroutineAllowed = false;

        p0 = designatedRoute.GetChild(0).position;
        p1 = designatedRoute.GetChild(1).position;
        p2 = designatedRoute.GetChild(2).position;
        p3 = designatedRoute.GetChild(3).position;

        while (tParam < 1)
        {
            tParam += Time.deltaTime * speedModifier;
            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 + 3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 + 3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 + Mathf.Pow(tParam, 3) * p3;
            transform.position = objectPosition;
            //transform.LookAt(endPoint);
            yield return new WaitForEndOfFrame();
        }

        tParam = 0f;
        Debug.Log("Projectile has reached the end point");
        combatCore.SpawnedPlayer.ProjectileCoroutineAllowed = true;
        combatCore.SpawnedPlayer.ProcessHitOrMiss();
        coroutineAllowed = true;

    }
}
