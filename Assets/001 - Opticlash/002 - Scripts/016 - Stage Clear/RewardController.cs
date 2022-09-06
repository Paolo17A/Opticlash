using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardController : MonoBehaviour
{
    public Image RewardImage;
    public TextMeshProUGUI RewardTMP;

    private void Start()
    {
        HideReward();
    }

    public void HideReward()
    {
        gameObject.SetActive(false);
    }

    public void ShowReward()
    {
        gameObject.SetActive(true);
    }
}
