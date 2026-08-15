using UnityEngine;

public class InteractableObject : MonoBehaviour, IPlayerInteractable
{
    [Header("Interaction")]
    [SerializeField] private string displayName = string.Empty;
    [SerializeField] private string interactionPrompt = "Explore";

    [TextArea(3, 8)]
    public string informationText;

    public string InteractionName => string.IsNullOrWhiteSpace(displayName)
        ? NicifyName(gameObject.name)
        : displayName.Trim();

    public string InteractionPrompt => string.IsNullOrWhiteSpace(interactionPrompt)
        ? $"Explore {InteractionName}"
        : $"{interactionPrompt.Trim()} {InteractionName}";

    public string InformationText => informationText;
    public bool CanInteract => isActiveAndEnabled && !string.IsNullOrWhiteSpace(informationText);

    public void Interact()
    {
        if (!CanInteract)
        {
            return;
        }

        UIManager uiManager = UIManager.Instance != null
            ? UIManager.Instance
            : FindFirstObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.ShowInformation(InteractionName, informationText, this);
        }
        else
        {
            Debug.LogError("UIManager was not found in the scene!");
        }
    }

    private static string NicifyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Point of Interest";
        }

        return value.Replace('_', ' ').Trim();
    }
}
