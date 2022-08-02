using MyBox;
using Newtonsoft.Json;
//using PlayFab;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //===========================================================
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                    _instance = new GameObject().AddComponent<GameManager>();
            }

            return _instance;
        }
    }
    [field: SerializeField]public List<GameObject> GameMangerObj { get; set; }

    [field: SerializeField] public bool DebugMode { get; set; }
    [SerializeField] private string SceneToLoad;
    [field: SerializeField] public bool PanelActivated { get; set; }

    [field: Header("CAMERA")]
    [field: SerializeField] public Camera MyUICamera { get; set; }
    [field: SerializeField] public Camera MainCamera { get; set; }

    [field: Header("ERROR")]
    [field: SerializeField] private GameObject DualLogInErrorPanel { get; set; }
    [field: SerializeField] private GameObject ErrorPanel { get; set; }
    [field: SerializeField] private TextMeshProUGUI ErrorTMP { get; set; }

    [field: Header("MISCELLANEOUS SCRIPTS")]
    [field: SerializeField] public AudioManager AudioManager { get; set; }
    //[field: SerializeField] public InventoryManager InventoryManager { get; set; }
    [field: SerializeField] public SceneController SceneController { get; set; }
    //===========================================================

    private void Awake()
    {
        //Advertisements.Instance.Initialize();
        if (_instance != null)
        {
            for (int a = 0; a < GameMangerObj.Count; a++)
                Destroy(GameMangerObj[a]);
        }

        for (int a = 0; a < GameMangerObj.Count; a++)
            DontDestroyOnLoad(GameMangerObj[a]);
    }

    private void Start()
    {
        //Advertisements.Instance.Initialize();
        if (DebugMode)
            SceneController.CurrentScene = SceneToLoad;
        else
        {
            SceneController.CurrentScene = "EntryScene";
            /*if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
                PlayFabSettings.TitleId = "C1147";*/
        }
    }

    #region ERRORS
    public void DisplayDualLoginErrorPanel()
    {
        DualLogInErrorPanel.SetActive(true);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void DisplayErrorPanel(string errorMessage)
    {
        ErrorPanel.SetActive(true);
        PanelActivated = true;
        ErrorTMP.text = errorMessage;
    }

    public void CloseErrorPanel()
    {
        ErrorPanel.SetActive(false);
        PanelActivated = false;
    }
    #endregion

    public string DateTimeToJSONString(DateTime dateTime)
    {
        return SerializeIntValue(new List<string>()
            {
                "Month",
                "Day",
                "Year",
                "Hour",
                "Minute",
                "Second"
            },
            new List<int>()
            {
                dateTime.Month,
                dateTime.Day,
                dateTime.Year,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second
            });
    }

    public string TimeSpanToJSONString(TimeSpan timeSpan)
    {
        return SerializeIntValue(new List<string>()
        {
            "Days",
            "Hours",
            "Minutes",
            "Seconds"
        }, new List<int>()
        {
            timeSpan.Days,
            timeSpan.Hours,
            timeSpan.Minutes,
            timeSpan.Seconds
        });
    }

    public DateTime JSONStringToDateTime(string serializedString)
    {
        return new DateTime(DeserializeIntValue(serializedString, "Year"), 
            DeserializeIntValue(serializedString, "Month"),
            DeserializeIntValue(serializedString, "Day"),
            DeserializeIntValue(serializedString, "Hour"),
            DeserializeIntValue(serializedString, "Minute"),
            DeserializeIntValue(serializedString, "Second"));
    }
    
    public TimeSpan JSONStringToTimeSpan(string serializedString)
    {
        return new TimeSpan(DeserializeIntValue(serializedString, "Days"),
            DeserializeIntValue(serializedString, "Hours"), 
            DeserializeIntValue(serializedString, "Minutes"),
            DeserializeIntValue(serializedString, "Seconds"));
    }

    public string DeserializeStringValue(string value, string key)
    {
        Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

        return result[key];
    }
    
    public int DeserializeIntValue(string value, string key)
    {
        Dictionary<string, int> result = JsonConvert.DeserializeObject<Dictionary<string, int>>(value);

        return result[key];
    }

    public float DeserializeFloatValue(string value, string key)
    {
        Dictionary<string, float> result = JsonConvert.DeserializeObject<Dictionary<string, float>>(value);

        return result[key];
    }

    public bool DeserializeBoolValue(string value, string key)
    {
        Dictionary<string, bool> result = JsonConvert.DeserializeObject<Dictionary<string, bool>>(value);

        return result[key];
    }

    public string SerializeStringValue(List<string> keyValues, List<string> values)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        for (int a = 0; a < keyValues.Count; a++)
            dict.Add(keyValues[a], values[a]);

        return JsonConvert.SerializeObject(dict);
    }

    public string SerializeIntValue(List<string> keyValues, List<int> values)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();

        for (int a = 0; a < keyValues.Count; a++)
            dict.Add(keyValues[a], values[a]);

        return JsonConvert.SerializeObject(dict);
    }

    public string SerializeFloatValue(List<string> keyValues, List<float> values)
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();

        for (int a = 0; a < keyValues.Count; a++)
            dict.Add(keyValues[a], values[a]);

        return JsonConvert.SerializeObject(dict);
    }
}
