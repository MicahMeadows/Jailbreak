using UnityEngine;

public class Laser : MonoBehaviour
{
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(0, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit))
        {
            if (hit.collider)
            {
                lr.SetPosition(1, hit.point);
            }
            if (hit.transform.tag == "ComputerPlayer")
            {
                Player player = hit.transform.GetComponent<Player>();
                player.OnHitByLaser();
            }
        }
        else
        {
            lr.SetPosition(1, transform.position + (-transform.right * 25));
        }
    }
}
