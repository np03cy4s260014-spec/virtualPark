public interface IPlayerInteractable
{
    string InteractionName { get; }
    string InteractionPrompt { get; }
    string InformationText { get; }
    bool CanInteract { get; }

    void Interact();
}
