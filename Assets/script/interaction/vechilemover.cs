using UnityEngine;

public class ParkVehicleMover : MonoBehaviour
{
    [Header("Route")]
    public Transform[] waypoints;
    public float speed = 3f;
    public float rotationSpeed = 5f;
    [SerializeField, Min(0.05f)] private float reachDistance = 0.5f;
    [SerializeField] private bool pingPong = false;

    private int currentWaypoint;
    private int routeDirection = 1;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || speed <= 0f)
        {
            return;
        }

        Transform target = waypoints[currentWaypoint];
        if (target == null)
        {
            AdvanceWaypoint();
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > reachDistance * reachDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * 90f * Time.deltaTime);

            Vector3 destination = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        }
        else
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        if (waypoints == null || waypoints.Length <= 1)
        {
            currentWaypoint = 0;
            return;
        }

        if (pingPong)
        {
            if (currentWaypoint >= waypoints.Length - 1)
            {
                routeDirection = -1;
            }
            else if (currentWaypoint <= 0)
            {
                routeDirection = 1;
            }

            currentWaypoint += routeDirection;
        }
        else
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
