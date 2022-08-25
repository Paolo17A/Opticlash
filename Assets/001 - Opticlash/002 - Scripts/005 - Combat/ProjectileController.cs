using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private CombatCore combatCore;

    [Header("ROUTE")]
    [SerializeField] private Transform highRoute;
    [SerializeField] private Transform lowRoute;
    [SerializeField][ReadOnly] private Transform designatedRoute;
    [SerializeField][ReadOnly] private Transform endPoint;
    [SerializeField][ReadOnly] private float tParam;
    [SerializeField][ReadOnly] private Vector2 objectPosition;
    [SerializeField] private float speedModifier;
    [SerializeField] private float rotationSpeed;
    [SerializeField][ReadOnly] private bool coroutineAllowed;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private Vector2 p0;
    [SerializeField][ReadOnly] private Vector2 p1;
    [SerializeField][ReadOnly] private Vector2 p2;
    [SerializeField][ReadOnly] private Vector2 p3;
    [SerializeField][ReadOnly] private int childrenPassed;

    // Start is called before the first frame update
    private void Start()
    {
        tParam = 0f;
        coroutineAllowed = true;
    }

    public IEnumerator GoByTheRoute()
    {
        combatCore.SpawnedPlayer.ProjectileCoroutineAllowed = false;
        coroutineAllowed = false;
        if(combatCore.CurrentEnemy.ThisMonsterPlacement == EnemyCombatController.MonsterPlacement.HIGH)
        {
            designatedRoute = highRoute;
            endPoint = highRoute.GetChild(3);
        }
        else
        {
            designatedRoute = lowRoute;
            endPoint = lowRoute.GetChild(3);
        }
        p0 = designatedRoute.GetChild(0).position;
        p1 = designatedRoute.GetChild(1).position;
        p2 = designatedRoute.GetChild(2).position;
        p3 = designatedRoute.GetChild(3).position;

        childrenPassed = 0;
        while (tParam < 1)
        {
            tParam += Time.deltaTime * speedModifier;
            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 + 3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 + 3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 + Mathf.Pow(tParam, 3) * p3;
            transform.position = objectPosition;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, -50), rotationSpeed * Time.deltaTime);
            if(transform.position.x >= designatedRoute.GetChild(childrenPassed).position.x)
                childrenPassed++;
            
            yield return new WaitForEndOfFrame();
        }

        tParam = 0f;
        combatCore.SpawnedPlayer.ProjectileCoroutineAllowed = true;
        transform.rotation = Quaternion.identity;
        combatCore.SpawnedPlayer.ProcessHitOrMiss();
        coroutineAllowed = true;

    }
}
