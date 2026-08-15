using UnityEngine;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RepairGameplayScene()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
        if (uiManager == null)
        {
            GameObject uiObject = new GameObject("UI Manager");
            uiManager = uiObject.AddComponent<UIManager>();
        }

        uiManager.enabled = true;
        uiManager.Initialize();

        PauseManager pauseManager = Object.FindFirstObjectByType<PauseManager>(FindObjectsInactive.Include);
        if (pauseManager == null)
        {
            GameObject pauseObject = new GameObject("Pause Manager");
            pauseManager = pauseObject.AddComponent<PauseManager>();
        }

        pauseManager.enabled = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Virtual Park could not find a Player-tagged character in the active scene.");
            return;
        }

        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        if (interaction == null)
        {
            interaction = player.AddComponent<PlayerInteraction>();
        }

        if (interaction.playerCamera == null)
        {
            interaction.playerCamera = Camera.main;
        }
    }
}
