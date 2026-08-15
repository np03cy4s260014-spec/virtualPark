using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Detection")]
    public Camera playerCamera;
    public float interactionDistance = 5f;
    [SerializeField, Range(0.01f, 0.5f)] private float aimAssistRadius = 0.12f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private readonly RaycastHit[] raycastHits = new RaycastHit[16];
    private UIManager uiManager;

    private void Awake()
    {
        if (aimAssistRadius <= 0f)
        {
            aimAssistRadius = 0.12f;
        }

        if (interactionLayers.value == 0)
        {
            interactionLayers = ~0;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance != null
                ? UIManager.Instance
                : FindFirstObjectByType<UIManager>();
        }

        if (uiManager == null)
        {
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null || UIManager.IsPaused || uiManager.IsInformationVisible)
        {
            uiManager.SetFocusedInteractable(null);
            return;
        }

        uiManager.SetFocusedInteractable(FindFocusedInteractable());
    }

    private MonoBehaviour FindFocusedInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.SphereCastNonAlloc(
            ray,
            aimAssistRadius,
            raycastHits,
            interactionDistance,
            interactionLayers,
            QueryTriggerInteraction.Collide);

        float nearestInteractableDistance = float.PositiveInfinity;
        float nearestBlockingDistance = float.PositiveInfinity;
        MonoBehaviour nearestInteractable = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = raycastHits[i];
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            MonoBehaviour candidate = FindInteractable(hit.collider);
            if (candidate != null)
            {
                if (hit.distance < nearestInteractableDistance)
                {
                    nearestInteractableDistance = hit.distance;
                    nearestInteractable = candidate;
                }

                continue;
            }

            if (!hit.collider.isTrigger && hit.distance < nearestBlockingDistance)
            {
                nearestBlockingDistance = hit.distance;
            }
        }

        return nearestInteractableDistance <= nearestBlockingDistance + 0.05f
            ? nearestInteractable
            : null;
    }

    private static MonoBehaviour FindInteractable(Collider hitCollider)
    {
        MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPlayerInteractable interactable && interactable.CanInteract)
            {
                return behaviours[i];
            }
        }

        behaviours = hitCollider.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPlayerInteractable interactable && interactable.CanInteract)
            {
                return behaviours[i];
            }
        }

        return null;
    }

    private void OnDisable()
    {
        if (uiManager != null)
        {
            uiManager.SetFocusedInteractable(null);
        }
    }
}
