using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Legacy scene reference (optional)")]
    public GameObject pausePanel;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager uiManager = UIManager.Instance != null
                ? UIManager.Instance
                : FindFirstObjectByType<UIManager>();

            if (!IsPaused && uiManager != null && uiManager.CloseInformationIfOpen())
            {
                return;
            }

            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        IsPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        UIManager uiManager = UIManager.Instance != null
            ? UIManager.Instance
            : FindFirstObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.SetPaused(true);
        }
        else
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        IsPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        UIManager uiManager = UIManager.Instance != null
            ? UIManager.Instance
            : FindFirstObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.SetPaused(false);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}

