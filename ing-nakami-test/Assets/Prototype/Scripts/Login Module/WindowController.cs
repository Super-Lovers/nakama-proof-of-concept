using UnityEngine;

public abstract class WindowController : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }
}