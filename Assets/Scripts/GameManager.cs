using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TMP_Text timePlayedText;
    public static int score = 0;
    public static float timePlayed = 0f;
    public static int currScene = 0;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    // Update is called once per frame
    void Update()
    {
        // update time 
        timePlayed += Time.unscaledDeltaTime;

        // get mins played and seconds played
        int minutes = Mathf.FloorToInt(timePlayed / 60);
        int seconds = Mathf.FloorToInt(timePlayed % 60);

        // change time played text accordingly 
        if(timePlayedText)
            timePlayedText.text = "Time Played: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        Debug.Log(timePlayed);
    }

    public void GoToMainMenu()
    {
        SaveGame();
        currScene = 0;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToTutorial()
    {
        SaveGame();
        currScene = 1;
        SceneManager.LoadScene("Tutorial");
    }

    public void GoToLevel1()
    {
        SaveGame();
        currScene = 2;
        SceneManager.LoadScene("Level1");
    }
    public void GoToLevel2()
    {
        SaveGame();
        currScene = 3;
        SceneManager.LoadScene("Level2");
    }

    public void GoToLevel3()
    {
        SaveGame();
        currScene = 4;
        SceneManager.LoadScene("Level3");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    public void SaveGame()
    {
        PlayerPrefs.SetFloat("TimePlayed", timePlayed);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
    }

    // Load the data from PlayerPrefs
    public void LoadGame()
    {
        timePlayed = PlayerPrefs.GetFloat("TimePlayed", 0f);
        score = PlayerPrefs.GetInt("Score", 0);
    }

    // Optional: clear data for debugging/reset
    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("TimePlayed");
        PlayerPrefs.DeleteKey("Score");
        PlayerPrefs.Save();

        timePlayed = 0f;
        score = 0;
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}