using UnityEngine;

public class DroneController : MonoBehaviour
{
    private DroneControl drone;


    private void FindDrone()
    {
        if (drone == null)
        {
            drone = GameObject.FindGameObjectWithTag("Drone").GetComponent<DroneControl>();
        }
    }

    public void OnForward()
    {
        FindDrone();
        if (drone)
        {
            drone.OnForward();
        }
    }

    public void OffForward()
    {

        FindDrone();
        if (drone)
        {
            drone.OffForward();
        }
    }

    public void OnBackward()
    {

        FindDrone();
        if (drone)
        {
            drone.OnBackward();
        }
    }

    public void OffBackward()
    {

        FindDrone();
        if (drone)
        {
            drone.OffBackward();
        }
    }

    public void OnLeft()
    {
        FindDrone();
        if (drone)
        {
            drone.OnLeft();
        }
    }

    public void OffLeft()
    {
        FindDrone();
        if (drone)
        {
            drone.OffLeft();
        }
    }

    public void OnRight()
    {
        FindDrone();
        if (drone)
        {
            drone.OnRight();
        }
    }

    public void OffRight()
    {
        FindDrone();
        if (drone)
        {
            drone.OffRight();
        }
    }
}
