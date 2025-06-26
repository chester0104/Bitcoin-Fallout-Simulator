// preedit-code/LevelManager.cs
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject player;
    public static bool IsPlaying { get; private set; }
    public bool isFinalLevel = false;
    public AudioClip startSFX;
    public AudioClip startSFX2;
    public AudioClip winSFX;
    public AudioClip loseSFX;
    public GameObject winScreen;
    public GameObject loseScreen;
    public TMP_Text winText;
    public TMP_Text restartOrNextText;

    [Header("Resources Settings")]
    public static int batteryCount = 0;
    public static int gearsCount = 0;
    public int score = 0;
    public TMP_Text scoreText;
    public GameObject levelText;
    public WaveSpawner ws;
    AudioSource audioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (ws == null)
            ws = GameObject.FindGameObjectWithTag("WaveSpawner").GetComponent<WaveSpawner>();
    }

    void Start()
    {
        IsPlaying = true;
        StartCoroutine(PlayStartSFXs());
        StartCoroutine(ShowLevelText());
    }

    public void LoadNextLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Level3")
        {
            SceneManager.LoadScene("Level1");
        }
        else
        {
            String currentSceneNumber = scene.name.ToString().Substring(5);
            SceneManager.LoadScene("Level" + currentSceneNumber);
        }
    }

    public IEnumerator PlaySoundForDuration(AudioClip clip, float duration)
    {
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }

    IEnumerator ShowLevelText()
    {
        levelText.SetActive(true);
        yield return new WaitForSeconds(3f);
        levelText.SetActive(false);
    }

    public void LevelLost()
    {
        // Stop time
        Time.timeScale = 0f;
        
        // Stop player's AudioSource if it exists
        if (player != null)
        {
            AudioSource playerAudioSource = player.GetComponent<AudioSource>();
            if (playerAudioSource != null)
            {
                playerAudioSource.Stop();
            }
        }
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        loseScreen.SetActive(true);
        IsPlaying = false;
        StartCoroutine(PlaySoundForDuration(loseSFX, 10.0f));
    }

    public void OnWavesCompleted()
    {
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isFinalLevel)
        {
            winText.text = "You Beat the Game!";
            restartOrNextText.text = "Restart";
            winScreen.SetActive(true);
            PlaySoundClip(winSFX);
        }
        else
        {
            Scene scene = SceneManager.GetActiveScene();
            string currentSceneNumber = scene.name.Substring(5);
            StartCoroutine(PlaySoundForDuration(winSFX, 10.0f));
            winText.text = "Level " + currentSceneNumber + " Completed!";
            restartOrNextText.text = "Next Level";
            winScreen.SetActive(true);
        }
    }

    public void ReloadSameScene()
    {
        Debug.Log("Reloading scene...");
        // Reset time scale before reloading
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void PlaySoundClip(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void AddBatteries()
    {
        batteryCount++;
    }
    
    public void AddGears()
    {
        gearsCount++;
    }

    public void UpdateScore(int scoreIncrease)
    {
        score += scoreIncrease;
        scoreText.text = "Score: " + score.ToString();
    }

    IEnumerator PlayStartSFXs()
    {
        AudioSource.PlayClipAtPoint(startSFX, player.transform.position);
        yield return new WaitForSeconds(3f);
        AudioSource.PlayClipAtPoint(startSFX2, player.transform.position);
    }
}