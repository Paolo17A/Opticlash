using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaboomController : MonoBehaviour
{
    [field: SerializeField] PlayerData PlayerData { get; set; }

    public void HideKaboom()
    {
        gameObject.SetActive(false);
    }
}
