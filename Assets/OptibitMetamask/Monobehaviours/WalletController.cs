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
using UnityEngine;

public class WalletController : MonoBehaviour
{
    [SerializeField] private LoginCore loginCore;
    [SerializeField] private string metamaskPackageName;
    [SerializeField] private string walletLoginURL;

    //  ===============================================

    private string userWallet;

    //  ===============================================

    private void Awake()
    {
        Application.deepLinkActivated += onDeepLinkActivated;
    }

    private void onDeepLinkActivated(string url)
    {
        Debug.Log("betlog");
        userWallet = url.Split("?"[0])[1];

        if (!userWallet.StartsWith("0x"))
        {
            //  ERROR HERE IF RESPONSE DOES NOT STARTS WITH 0x
            //  LOGIC HERE....
            GameManager.Instance.DisplayErrorPanel("Invalid Wallet ID. Pls try logging in using Metamask");
            return;
        }

        //  LOGIC HERE AFTER GETTING THE USER WALLET ADDRESS
        Debug.Log(userWallet);
        //loginCore.LoginWithMetaMask(userWallet);
    }

    /* NOTE:
     * 
     * CREATE A CUSTOM MAIN MANIFEST UNDER PROJECT SETTINGS -> PUBLISHING SETTINGS ->
     * CHECK CUSTOM MAIN MANIFEST
     * 
     * THEN GO TO PLUGINS/ANDROID/ANDROIDMANIFEST.XML AND PUT THIS CODE THERE
     * 
     * <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="optibit" android:host="web3Wallet" />
      </intent-filter>
     */
    public void ConnectWallet()
    {
        //  PUT ANY LOADING INTERACTION HERE
        Wallet.WalletConnect(walletLoginURL, metamaskPackageName);
    }
}
