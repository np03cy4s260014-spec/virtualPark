using UnityEngine;

public class POIInteraction : MonoBehaviour, IPlayerInteractable
{
    [Header("Point of Interest")]
    [SerializeField] private string displayName = string.Empty;
    [SerializeField] private string interactionPrompt = "Discover";

    [TextArea(3, 8)]
    public string informationText;

    [Header("Legacy UI (no longer required)")]
    public GameObject informationPanel;
    public TMPro.TMP_Text displayText;

    private bool playerInside;
    private UIManager uiManager;

    public string InteractionName => string.IsNullOrWhiteSpace(displayName)
        ? NicifyName(gameObject.name)
        : displayName.Trim();

    public string InteractionPrompt => string.IsNullOrWhiteSpace(interactionPrompt)
        ? $"Discover {InteractionName}"
        : $"{interactionPrompt.Trim()} {InteractionName}";

    public string InformationText => informationText;
    public bool CanInteract => isActiveAndEnabled && playerInside && !string.IsNullOrWhiteSpace(informationText);

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 4f;
            trigger = sphere;
        }

        trigger.isTrigger = true;

        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            RegisterWithUI(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            RegisterWithUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            RegisterWithUI(false);
        }
    }

    public void Interact()
    {
        if (!CanInteract)
        {
            return;
        }

        ResolveUIManager();
        if (uiManager != null)
        {
            uiManager.ShowInformation(InteractionName, informationText, this);
        }
    }

    private void RegisterWithUI(bool isNearby)
    {
        ResolveUIManager();
        if (uiManager != null)
        {
            uiManager.SetNearbyInteractable(this, isNearby);
        }
    }

    private void ResolveUIManager()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance != null
                ? UIManager.Instance
                : FindFirstObjectByType<UIManager>();
        }
    }

    private void OnDisable()
    {
        playerInside = false;
        RegisterWithUI(false);
    }

    private static string NicifyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Point of Interest";
        }

        string cleaned = value.Replace('_', ' ').Trim();
        if (cleaned.StartsWith("poi ", System.StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(4);
        }

        return cleaned;
    }
}
