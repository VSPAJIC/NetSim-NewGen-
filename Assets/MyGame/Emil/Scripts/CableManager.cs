using UnityEngine;

public class CableManager : MonoBehaviour
{
    public Camera cam;
    public GameObject cablePrefab;

    private Cable currentCable;
    private bool isPlacingCable = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryConnect();
        }
    }

    void TryConnect()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) return;

        GameObject obj = hit.collider.gameObject;

        // ===== START KABEL =====
        if (!isPlacingCable && obj.CompareTag("Port"))
        {
            GameObject newCable = Instantiate(cablePrefab);
            currentCable = newCable.GetComponent<Cable>();

            currentCable.SetPortA(obj);
            isPlacingCable = true;
            return;
        }

        // ===== WAYPOINT =====
        if (isPlacingCable && obj.CompareTag("Waypoint"))
        {
            currentCable.AddWaypoint(obj);
            return;
        }

        // ===== ENDE =====
        if (isPlacingCable && obj.CompareTag("Port"))
        {
            if (obj == currentCable.portA)
                return;

            currentCable.SetPortB(obj);

            isPlacingCable = false;
            currentCable = null;
        }
    }
}