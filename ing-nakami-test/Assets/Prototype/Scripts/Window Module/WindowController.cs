using UnityEngine;

public abstract class WindowController : MonoBehaviour
{
    public void Close(Object obj)
    {
        GameObject gameObj = (GameObject)obj;
        gameObj.SetActive(false);
    }

    public void Open(Object obj)
    {
        GameObject gameObj = (GameObject)obj;
        gameObj.SetActive(true);
    }
}