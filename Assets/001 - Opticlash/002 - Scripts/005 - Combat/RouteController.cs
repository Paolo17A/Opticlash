using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteController : MonoBehaviour
{
    [field: SerializeField] private Transform[] ControlPoints { get; set; }
    private Vector2 gizmoPosition;

    private void OnDrawGizmos()
    {
        Debug.Log("drawing gizos");
        for(float t = 0; t <= 1; t += 0.05f)
        {
            gizmoPosition = Mathf.Pow(1 - t, 3) * ControlPoints[0].position + 
                3 * Mathf.Pow(1 - t, 2) * t * ControlPoints[1].position + 
                3 * (1 - t) * Mathf.Pow(t, 2) * ControlPoints[2].position + 
                Mathf.Pow(t, 3) * ControlPoints[3].position;

            Gizmos.DrawSphere(gizmoPosition, 0.05f);
        }

        Gizmos.DrawLine(new Vector2(ControlPoints[0].position.x, ControlPoints[0].position.y), new Vector2(ControlPoints[1].position.x, ControlPoints[1].position.y));
        Gizmos.DrawLine(new Vector2(ControlPoints[2].position.x, ControlPoints[2].position.y), new Vector2(ControlPoints[3].position.x, ControlPoints[3].position.y));
    }
}
