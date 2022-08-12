/*  =================================================================
 *  |   META MASK SDK FOR CREATIVE BRAIN STUDIO                     |
 *  |   PLEASE DON'T DISTRIBUTE THIS WITHOUT THE PERMISSION         |
 *  |   OF ANY BOARD DIRECTORS OR THE CTO                           |
 *  |                                                               |
 *  |   HAVE FUN USING THIS !!                                      |
 *  =================================================================
 */
//  URL FOR OPTIBIT
//  optibit.herokuapp.com/tester (LOGIN)
//
//  PACKAGE NAME FOR METAMASK
//  io.metamask

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Wallet
{
    private static bool CheckPackageAppIsPresent(string package)
    {
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

        //take the list of all packages on the device
        AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>("getInstalledPackages", 0);
        int num = appList.Call<int>("size");
        for (int i = 0; i < num; i++)
        {
            AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>("get", i);
            string packageNew = appInfo.Get<string>("packageName");
            if (packageNew.CompareTo(package) == 0)
            {
                return true;
            }
        }
        return false;
    }

    public static void WalletConnect(string urlWallet, string appPackage)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (!CheckPackageAppIsPresent(appPackage))
        {
            Debug.Log("You don't have metamask app installed! Please install it first then login.");
            GameManager.Instance.DisplayErrorPanel("You don't have metamask app installed! Please install it first then login.");
            return;
        }

        Application.OpenURL("dapp://" + urlWallet);
#else
        Application.OpenURL("https://" + urlWallet);
#endif
    }
}
