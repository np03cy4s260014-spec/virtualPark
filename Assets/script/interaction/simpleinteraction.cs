using UnityEngine;

public class SimpleInteraction : MonoBehaviour
{
    public GameObject informationPanel;

    private bool playerInside = false;

    void Update()
    {
        if (playerInside && informationPanel != null && Input.GetKeyDown(KeyCode.E))
        {
            informationPanel.SetActive(!informationPanel.activeSelf);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Press E to interact");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (informationPanel != null)
            {
                informationPanel.SetActive(false);
            }
        }
    }
}
