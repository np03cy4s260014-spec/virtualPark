using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    [Header("Legacy scene references (optional)")]
    public GameObject informationPanel;
    public TMP_Text informationText;

    public bool IsInformationVisible => informationPanel != null && informationPanel.activeSelf;

    private readonly List<MonoBehaviour> nearbyInteractables = new List<MonoBehaviour>();
    private MonoBehaviour focusedInteractable;
    private int focusedFrame = -10;

    private Canvas generatedCanvas;
    private GameObject promptPanel;
    private TMP_Text promptText;
    private GameObject controlsPanel;
    private GameObject reticle;
    private GameObject pausePanel;
    private TMP_Text informationTitle;
    private GameObject toastPanel;
    private TMP_Text toastText;
    private Coroutine toastRoutine;
    private Transform playerTransform;

    private static readonly Color ParkGreen = new Color32(51, 185, 128, 255);
    private static readonly Color DeepNavy = new Color32(13, 25, 38, 246);
    private static readonly Color PanelNavy = new Color32(21, 38, 54, 246);
    private static readonly Color SoftWhite = new Color32(241, 247, 244, 255);
    private static readonly Color MutedWhite = new Color32(194, 210, 207, 255);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        Initialize();
    }

    private void Start()
    {
        ApplyGameplayInputState(true);
        ShowToast("Welcome to Virtual Park  •  Explore the landmarks", 4f);
    }

    private void Update()
    {
        if (IsPaused)
        {
            return;
        }

        if (IsInformationVisible)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideInformation();
            }

            return;
        }

        MonoBehaviour candidate = ResolveCurrentInteractable();
        UpdatePrompt(candidate);

        if (candidate != null && Input.GetKeyDown(KeyCode.E))
        {
            if (candidate is IPlayerInteractable interactable && interactable.CanInteract)
            {
                interactable.Interact();
            }
        }
    }

    public void Initialize()
    {
        if (generatedCanvas != null)
        {
            return;
        }

        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }

        BuildRuntimeUI();
    }

    public void SetFocusedInteractable(MonoBehaviour candidate)
    {
        focusedInteractable = candidate;
        focusedFrame = Time.frameCount;
    }

    public void SetNearbyInteractable(MonoBehaviour candidate, bool isNearby)
    {
        if (candidate == null)
        {
            return;
        }

        if (isNearby)
        {
            if (!nearbyInteractables.Contains(candidate))
            {
                nearbyInteractables.Add(candidate);
            }
        }
        else
        {
            nearbyInteractables.Remove(candidate);
            if (focusedInteractable == candidate)
            {
                focusedInteractable = null;
            }
        }
    }

    public void ShowInformation(string message)
    {
        ShowInformation("Point of Interest", message, null);
    }

    public void ShowInformation(string title, string message, MonoBehaviour owner)
    {
        if (IsPaused || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        informationTitle.text = string.IsNullOrWhiteSpace(title) ? "POINT OF INTEREST" : title.ToUpperInvariant();
        informationText.text = message.Trim();
        informationPanel.SetActive(true);

        promptPanel.SetActive(false);
        controlsPanel.SetActive(false);
        reticle.SetActive(false);
        Time.timeScale = 0f;
        ApplyGameplayInputState(false);
    }

    public void HideInformation()
    {
        if (informationPanel == null || !informationPanel.activeSelf)
        {
            return;
        }

        informationPanel.SetActive(false);

        if (!IsPaused)
        {
            Time.timeScale = 1f;
            controlsPanel.SetActive(true);
            reticle.SetActive(true);
            ApplyGameplayInputState(true);
        }
    }

    public bool CloseInformationIfOpen()
    {
        if (!IsInformationVisible)
        {
            return false;
        }

        HideInformation();
        return true;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;

        if (paused)
        {
            if (IsInformationVisible)
            {
                informationPanel.SetActive(false);
            }

            pausePanel.SetActive(true);
            promptPanel.SetActive(false);
            controlsPanel.SetActive(false);
            reticle.SetActive(false);
            Time.timeScale = 0f;
            ApplyGameplayInputState(false);
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            controlsPanel.SetActive(true);
            reticle.SetActive(true);
            ApplyGameplayInputState(true);
        }
    }

    public void ShowToast(string message, float duration = 2.5f)
    {
        if (toastPanel == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        toastRoutine = StartCoroutine(ShowToastRoutine(message, duration));
    }

    private IEnumerator ShowToastRoutine(string message, float duration)
    {
        toastText.text = message;
        toastPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, duration));

        toastPanel.SetActive(false);
        toastRoutine = null;
    }

    private MonoBehaviour ResolveCurrentInteractable()
    {
        if (Time.frameCount - focusedFrame <= 1 && IsValidInteractable(focusedInteractable))
        {
            return focusedInteractable;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        MonoBehaviour nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            MonoBehaviour candidate = nearbyInteractables[i];
            if (!IsValidInteractable(candidate))
            {
                nearbyInteractables.RemoveAt(i);
                continue;
            }

            float distance = playerTransform == null
                ? i
                : (candidate.transform.position - playerTransform.position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private static bool IsValidInteractable(MonoBehaviour candidate)
    {
        return candidate != null
            && candidate.isActiveAndEnabled
            && candidate is IPlayerInteractable interactable
            && interactable.CanInteract;
    }

    private void UpdatePrompt(MonoBehaviour candidate)
    {
        if (candidate == null || !(candidate is IPlayerInteractable interactable))
        {
            promptPanel.SetActive(false);
            return;
        }

        promptText.text = interactable.InteractionPrompt;
        promptPanel.SetActive(true);
    }

    private void BuildRuntimeUI()
    {
        GameObject canvasObject = new GameObject(
            "Virtual Park HUD",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        canvasObject.transform.SetParent(transform, false);

        generatedCanvas = canvasObject.GetComponent<Canvas>();
        generatedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        generatedCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        BuildControls(canvasRect);
        BuildReticle(canvasRect);
        BuildPrompt(canvasRect);
        BuildInformationPanel(canvasRect);
        BuildPausePanel(canvasRect);
        BuildToast(canvasRect);
    }

    private void BuildControls(RectTransform parent)
    {
        controlsPanel = CreatePanel("Controls", parent, new Color32(13, 25, 38, 218));
        SetFixedRect(controlsPanel.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(310f, 224f), new Vector2(0f, 1f));

        TMP_Text brand = CreateText("Brand", controlsPanel.transform, "VIRTUAL PARK", 26f, FontStyles.Bold, ParkGreen, TextAlignmentOptions.TopLeft);
        SetStretchRect(brand.rectTransform, new Vector2(22f, 150f), new Vector2(-22f, -18f));

        TMP_Text subtitle = CreateText("Subtitle", controlsPanel.transform, "SELF-GUIDED EXPLORATION", 12f, FontStyles.Bold, MutedWhite, TextAlignmentOptions.TopLeft);
        subtitle.characterSpacing = 2f;
        SetFixedRect(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(22f, -58f), new Vector2(270f, 24f), new Vector2(0f, 1f));

        TMP_Text controls = CreateText(
            "Controls List",
            controlsPanel.transform,
            "WASD   Walk\nSHIFT   Sprint\nSPACE   Jump\nE       Explore / Close\nN       Day / Night\nESC     Pause",
            17f,
            FontStyles.Normal,
            SoftWhite,
            TextAlignmentOptions.TopLeft);
        controls.lineSpacing = 8f;
        SetStretchRect(controls.rectTransform, new Vector2(22f, 18f), new Vector2(-22f, -88f));
    }

    private void BuildReticle(RectTransform parent)
    {
        reticle = CreatePanel("Reticle", parent, new Color32(240, 255, 248, 220));
        SetFixedRect(reticle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(7f, 7f), new Vector2(0.5f, 0.5f));
    }

    private void BuildPrompt(RectTransform parent)
    {
        promptPanel = CreatePanel("Interaction Prompt", parent, DeepNavy);
        SetFixedRect(promptPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 102f), new Vector2(610f, 72f), new Vector2(0.5f, 0f));

        GameObject keyBadge = CreatePanel("Key Badge", promptPanel.transform, ParkGreen);
        SetFixedRect(keyBadge.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(46f, 46f), new Vector2(0f, 0.5f));

        TMP_Text key = CreateText("Key", keyBadge.transform, "E", 22f, FontStyles.Bold, DeepNavy, TextAlignmentOptions.Center);
        SetStretchRect(key.rectTransform, Vector2.zero, Vector2.zero);

        promptText = CreateText("Prompt", promptPanel.transform, "Explore", 22f, FontStyles.Bold, SoftWhite, TextAlignmentOptions.MidlineLeft);
        SetStretchRect(promptText.rectTransform, new Vector2(84f, 10f), new Vector2(-22f, -10f));
        promptPanel.SetActive(false);
    }

    private void BuildInformationPanel(RectTransform parent)
    {
        GameObject overlay = CreatePanel("Information Overlay", parent, new Color32(5, 12, 18, 205));
        SetStretchRect(overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        GameObject card = CreatePanel("Information Card", overlay.transform, PanelNavy);
        SetFixedRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 470f), new Vector2(0.5f, 0.5f));

        GameObject accent = CreatePanel("Accent", card.transform, ParkGreen);
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.sizeDelta = new Vector2(8f, 0f);
        accentRect.anchoredPosition = Vector2.zero;

        TMP_Text eyebrow = CreateText("Eyebrow", card.transform, "DISCOVER VIRTUAL PARK", 13f, FontStyles.Bold, ParkGreen, TextAlignmentOptions.TopLeft);
        eyebrow.characterSpacing = 2.5f;
        SetFixedRect(eyebrow.rectTransform, new Vector2(0f, 1f), new Vector2(48f, -38f), new Vector2(700f, 26f), new Vector2(0f, 1f));

        informationTitle = CreateText("Title", card.transform, "POINT OF INTEREST", 35f, FontStyles.Bold, SoftWhite, TextAlignmentOptions.TopLeft);
        SetFixedRect(informationTitle.rectTransform, new Vector2(0f, 1f), new Vector2(48f, -76f), new Vector2(700f, 56f), new Vector2(0f, 1f));

        informationText = CreateText("Information", card.transform, string.Empty, 21f, FontStyles.Normal, MutedWhite, TextAlignmentOptions.TopLeft);
        informationText.textWrappingMode = TextWrappingModes.Normal;
        informationText.overflowMode = TextOverflowModes.Ellipsis;
        informationText.lineSpacing = 12f;
        SetStretchRect(informationText.rectTransform, new Vector2(48f, 98f), new Vector2(-48f, -145f));

        Button closeButton = CreateButton("Close Button", card.transform, "CLOSE   [E]", HideInformation);
        SetFixedRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(-48f, 36f), new Vector2(190f, 52f), new Vector2(1f, 0f));

        informationPanel = overlay;
        informationPanel.SetActive(false);
    }

    private void BuildPausePanel(RectTransform parent)
    {
        pausePanel = CreatePanel("Pause Overlay", parent, new Color32(5, 12, 18, 225));
        SetStretchRect(pausePanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        GameObject card = CreatePanel("Pause Card", pausePanel.transform, PanelNavy);
        SetFixedRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 480f), new Vector2(0.5f, 0.5f));

        TMP_Text eyebrow = CreateText("Eyebrow", card.transform, "VIRTUAL PARK", 14f, FontStyles.Bold, ParkGreen, TextAlignmentOptions.Center);
        eyebrow.characterSpacing = 3f;
        SetFixedRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(460f, 28f), new Vector2(0.5f, 1f));

        TMP_Text title = CreateText("Title", card.transform, "GAME PAUSED", 42f, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center);
        SetFixedRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(460f, 64f), new Vector2(0.5f, 1f));

        TMP_Text hint = CreateText("Hint", card.transform, "Take a moment. Your park visit is waiting.", 18f, FontStyles.Normal, MutedWhite, TextAlignmentOptions.Center);
        SetFixedRect(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(460f, 42f), new Vector2(0.5f, 1f));

        Button resume = CreateButton("Resume", card.transform, "RESUME", ResumeFromButton);
        SetFixedRect(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(360f, 58f), new Vector2(0.5f, 0.5f));

        Button restart = CreateButton("Restart", card.transform, "RESTART VISIT", RestartFromButton, false);
        SetFixedRect(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(360f, 58f), new Vector2(0.5f, 0.5f));

        Button quit = CreateButton("Quit", card.transform, "EXIT GAME", QuitFromButton, false);
        SetFixedRect(quit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -135f), new Vector2(360f, 58f), new Vector2(0.5f, 0.5f));

        TMP_Text escape = CreateText("Escape Hint", card.transform, "Press ESC to resume", 15f, FontStyles.Normal, MutedWhite, TextAlignmentOptions.Center);
        SetFixedRect(escape.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(420f, 28f), new Vector2(0.5f, 0f));

        pausePanel.SetActive(false);
    }

    private void BuildToast(RectTransform parent)
    {
        toastPanel = CreatePanel("Toast", parent, DeepNavy);
        SetFixedRect(toastPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(620f, 54f), new Vector2(0.5f, 1f));

        toastText = CreateText("Toast Text", toastPanel.transform, string.Empty, 17f, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center);
        SetStretchRect(toastText.rectTransform, new Vector2(20f, 8f), new Vector2(-20f, -8f));
        toastPanel.SetActive(false);
    }

    private void ResumeFromButton()
    {
        PauseManager manager = PauseManager.Instance != null
            ? PauseManager.Instance
            : FindFirstObjectByType<PauseManager>();

        if (manager != null)
        {
            manager.ResumeGame();
        }
        else
        {
            SetPaused(false);
        }
    }

    private void RestartFromButton()
    {
        PauseManager manager = PauseManager.Instance != null
            ? PauseManager.Instance
            : FindFirstObjectByType<PauseManager>();

        if (manager != null)
        {
            manager.RestartGame();
        }
    }

    private void QuitFromButton()
    {
        PauseManager manager = PauseManager.Instance != null
            ? PauseManager.Instance
            : FindFirstObjectByType<PauseManager>();

        if (manager != null)
        {
            manager.QuitGame();
        }
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.layer = LayerMask.NameToLayer("UI");
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        FontStyles style,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, UnityAction action, bool primary = true)
    {
        Color normalColor = primary ? ParkGreen : (Color)new Color32(38, 61, 76, 255);
        GameObject buttonObject = CreatePanel(name, parent, normalColor);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = primary ? new Color32(78, 218, 158, 255) : new Color32(55, 82, 99, 255);
        colors.pressedColor = new Color32(36, 153, 104, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(action);

        Color labelColor = primary ? DeepNavy : SoftWhite;
        TMP_Text buttonText = CreateText("Label", buttonObject.transform, label, 18f, FontStyles.Bold, labelColor, TextAlignmentOptions.Center);
        SetStretchRect(buttonText.rectTransform, new Vector2(12f, 4f), new Vector2(-12f, -4f));
        return button;
    }

    private static void SetFixedRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void SetStretchRect(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void ApplyGameplayInputState(bool gameplayEnabled)
    {
        StarterAssets.StarterAssetsInputs inputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = gameplayEnabled;
            inputs.cursorInputForLook = gameplayEnabled;

            if (!gameplayEnabled)
            {
                inputs.MoveInput(Vector2.zero);
                inputs.LookInput(Vector2.zero);
                inputs.JumpInput(false);
                inputs.SprintInput(false);
            }
        }

        Cursor.lockState = gameplayEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !gameplayEnabled;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }
}
