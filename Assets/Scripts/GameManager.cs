using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;


public class GameManager : MonoBehaviour
{
    public GameObject buttonPrefab;
    private string selectedLevel;
    public string playerName;
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DiscoverLevels();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RestartLevel(float delay)
    {
        StartCoroutine(RestartLevelDelay(delay));
    }

    private IEnumerator RestartLevelDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Game");
    }

    public List<PlayerTimeEntry> LoadPreviousTimes()
    {
        try
        {
            var scoresFile = Application.persistentDataPath + "/" + playerName + "_times.dat";

            using (var stream = File.Open(scoresFile, FileMode.Open))
            {
                var bin = new BinaryFormatter();
                var times = (List<PlayerTimeEntry>)bin.Deserialize(stream);
                return times;
            }
        }
        catch (IOException ex)
        {
            Debug.LogWarning("Couldn't load previous times for: " + playerName + ". Exception: " + ex.Message);
            return new List<PlayerTimeEntry>();
        }
    }

    public void SaveTime(decimal time)
    {
        var times = LoadPreviousTimes();
        var newTime = new PlayerTimeEntry();
        newTime.entryDate = DateTime.Now;
        newTime.time = time;
        var bFormatter = new BinaryFormatter();
        var filePath = Application.persistentDataPath + "/" + playerName + "_times.dat";

        using (var file = File.Open(filePath, FileMode.Create))
        {
            times.Add(newTime);
            bFormatter.Serialize(file, times);
        }
    }

    public void DisplayPreviousTimes()
    {
        var times = LoadPreviousTimes();
        var topThree = times.OrderBy(time => time.time).Take(3);

        var timesLabel = GameObject.Find("PreviousTimes").GetComponent<Text>();

        timesLabel.text = "BEST TIMES \n";

        foreach (var time in topThree)
        {
            timesLabel.text += time.entryDate.ToShortDateString() + ": " + time.time + "\n";
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadsceneMode)
    {
        if (!string.IsNullOrEmpty(selectedLevel)
        && scene.name == "Game")
        {
            Debug.Log("Loading level content for: " + selectedLevel);
            LoadLevelContent();
            DisplayPreviousTimes();
        }
        if (scene.name == "Menu")
        {
            DiscoverLevels();
        }
    }

    private void SetLevelName(string levelFilePath)
    {
        selectedLevel = levelFilePath;
        SceneManager.LoadScene("Game");
    }

    private void DiscoverLevels()
    {
        var levelPanelRectTransform = GameObject.Find("LevelItemsPanel").GetComponent<RectTransform>();

        var levelFiles = Directory.GetFiles(Application.dataPath, "*.json");

        var yOffset = 0f;

        for (var i = 0; i < levelFiles.Length; i++)
        {
            if (i == 0)
            {
                yOffset = -30;
            }
            else
            {
                yOffset -= 65f;
            }

            var levelFile = levelFiles[i];
            var levelName = Path.GetFileName(levelFile);

            var levelButtonObj = (GameObject)Instantiate(buttonPrefab, Vector2.zero, Quaternion.identity);

            var levelButtonRectTransform = levelButtonObj.GetComponent<RectTransform>();
            levelButtonRectTransform.SetParent(levelPanelRectTransform, true);

            levelButtonRectTransform.anchoredPosition = new Vector2(212.5f, yOffset);

            var levelButtonText = levelButtonObj.transform.GetChild(0).GetComponent<Text>();

            levelButtonText.text = levelName;

            var levelButton = levelButtonObj.GetComponent<Button>();
            levelButton.onClick.AddListener(delegate { SetLevelName(levelFile); });

            levelPanelRectTransform.sizeDelta = new Vector2(levelPanelRectTransform.sizeDelta.x, 60f * i);
        }

        levelPanelRectTransform.offsetMax = new Vector2(levelPanelRectTransform.offsetMax.x, 0f);
    }

    private void LoadLevelContent()
    {
        var existingLevelRoot = GameObject.Find("Level");
        Destroy(existingLevelRoot);
        var levelRoot = new GameObject("Level");

     
        var levelFileJsonContent = File.ReadAllText(selectedLevel);
        var levelData = JsonUtility.FromJson<LevelDataRepresentation>(
         levelFileJsonContent);
        
        foreach (var li in levelData.levelItems)
        {
           
            var pieceResource =
            Resources.Load("Prefabs/" + li.prefabName);
            if (pieceResource == null)
            {
                Debug.LogError("Cannot find resource: " + li.prefabName);
            }
            
            var piece = (GameObject)Instantiate(pieceResource,
            li.position, Quaternion.identity);
            var pieceSprite = piece.GetComponent<SpriteRenderer>();
            if (pieceSprite != null)
            {
                pieceSprite.sortingOrder = li.spriteOrder;
                pieceSprite.sortingLayerName = li.spriteLayer;
                pieceSprite.color = li.spriteColor;
            }
          
            piece.transform.parent = levelRoot.transform; 
            piece.transform.position = li.position;
            piece.transform.rotation = Quaternion.Euler(
            li.rotation.x, li.rotation.y, li.rotation.z);
            piece.transform.localScale = li.scale;
        }

        var SoyBoy = GameObject.Find("SoyBoy");
        SoyBoy.transform.position = levelData.playerStartPosition;
        Camera.main.transform.position = new Vector3(SoyBoy.transform.position.x, SoyBoy.transform.position.y, Camera.main.transform.position.z);

        // 1
        var camSettings = FindObjectOfType<CameraLerpToTransform>();
        // 2
        if (camSettings != null)
        {
           // camSettings.cameraZDepth = levelData.cameraSettings.cameraZDepth;
            camSettings.camTarget = GameObject.Find(
            levelData.cameraSettings.cameraTrackTarget).transform;
            camSettings.maxX = levelData.cameraSettings.maxX;
            camSettings.maxY = levelData.cameraSettings.maxY;
            camSettings.minX = levelData.cameraSettings.minX;
            camSettings.minY = levelData.cameraSettings.minY;
            camSettings.trackingSpeed =
            levelData.cameraSettings.trackingSpeed;
        }
    }
}
