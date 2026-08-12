using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [TextArea(3, 8)]
    public string informationText;

    public void Interact()
    {
        Debug.Log(informationText);
    }
}
