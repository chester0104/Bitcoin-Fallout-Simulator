using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuBehavior : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public static bool isGamePaused = false;
    private MouseLook mouseLook;

    void Start()
    {
        pauseMenuPanel.SetActive(false);

        mouseLook = FindFirstObjectByType<MouseLook>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);

        if (mouseLook != null)
            mouseLook.SetCursorState(false);
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);

        // Show cursor and unlock it for UI interaction
        if (mouseLook != null)
            mouseLook.SetCursorState(true);
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ChangeSensitivity(float sensitivity)
    {
        if (mouseLook != null)
        {
            mouseLook.mouseSens = Mathf.Clamp(mouseLook.mouseSens, 1, 1000);
            mouseLook.mouseSens = sensitivity;
        }
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading the main menu scene");
        // Reset time scale and cursor state before loading scene
        Time.timeScale = 1f;
        isGamePaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting the game");
        Application.Quit();
    }
}