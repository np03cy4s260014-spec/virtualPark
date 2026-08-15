using UnityEngine;

public class LampController : MonoBehaviour
{
    public Light lampLight;

    public void TurnOn()
    {
        if (lampLight != null)
        {
            lampLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        if (lampLight != null)
        {
            lampLight.enabled = false;
        }
    }
}
