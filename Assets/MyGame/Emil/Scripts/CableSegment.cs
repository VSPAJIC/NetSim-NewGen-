using UnityEngine;

public class CableSegment : MonoBehaviour
{
    public GameObject from;
    public GameObject to;

    public void Setup(GameObject start, GameObject end)
    {
        from = start;
        to = end;
    }
}