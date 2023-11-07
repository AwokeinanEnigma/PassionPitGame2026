#region

using Enigmaware.Motor;
using Pathfinding;
using UnityEngine;

#endregion

namespace Enigmaware.AI
{
    public class BasicAIMover : MonoBehaviour
    {
        public Transform targetPosition;

        public float speed = 2;

        public float nextWaypointDistance = 3;

        public bool reachedEndOfPath;
        private KinematicNPCMotor controller;

        private int currentWaypoint;

        public Path path;

        private Seeker seeker;

        public void Start()
        {
            seeker = GetComponent<Seeker>();
            // If you are writing a 2D game you should remove this line
            // and use the alternative way to move sugggested further below.
            controller = GetComponent<KinematicNPCMotor>();

            // Start a new path to the targetPosition, call the the OnPathComplete function
            // when the path has been calculated (which may take a few frames depending on the complexity)
            seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        }

        public float timer = 0;
        
        public void Update()
        {
            if (path == null)
                // We have no path to follow yet, so don't do anything
                return;

            timer += Time.deltaTime;
            if (timer > 1f)
            {
                timer = 0;
                seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
                controller.Jump();
            }
            
            // Check in a loop if we are close enough to the current waypoint to switch to the next one.
            // We do this in a loop because many waypoints might be close to each other and we may reach
            // several of them in the same frame.
            reachedEndOfPath = false;
            // The distance to the next waypoint in the path
            float distanceToWaypoint;
            while (true)
            {
                // If you want maximum performance you can check the squared distance instead to get rid of a
                // square root calculation. But that is outside the scope of this tutorial.
                distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    // Check if there is another waypoint or if we have reached the end of the path
                    if (currentWaypoint + 1 < path.vectorPath.Count)
                    {
                        currentWaypoint++;
                    }
                    else
                    {
                        // Set a status variable to indicate that the agent has reached the end of the path.
                        // You can use this to trigger some special code if your game requires that.
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            // Direction to the next waypoint
            // Normalize it so that it has a length of 1 world unit
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            // Multiply the direction by our desired speed to get a velocity
            
            Debug.DrawLine(path.vectorPath[currentWaypoint], transform.position, Color.magenta );
            
            // convert dir to wish direction 
            // convert wish direction to velocity
            // add velocity to controller
                
            
            controller.SetWishDirection(dir);
           // controller.AddVelocity( dir * speedFactor);
        }

        public void OnPathComplete(Path p)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

            if (!p.error)
            {
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                currentWaypoint = 0;
            }
        }
    }
}