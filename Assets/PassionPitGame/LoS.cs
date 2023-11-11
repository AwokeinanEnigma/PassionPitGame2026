using UnityEngine;
namespace PassionPitGame {
    public class LoS : MonoBehaviour {
        public float radius;
        [Range(0, 360)]
        public float angle;

        public GameObject playerRef;

        public LayerMask targetMask;
        public LayerMask obstructionMask;

        public bool canSeePlayer;

        public GameObject player;
        public float offset = -0.0168f;

        RaycastHit[] _raycastHits = new RaycastHit[1];
        public void Awake () {
            player = GameObject.Find("PlayerObject");
        }

        public bool CheckLOS () {
            Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, player.transform.position);
            //Debug.Log($"Distance to target is '{distanceToTarget}' and the direction is {directionToTarget}.");

            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
            {
                return true;
            }
            return false;
        }

        public Transform target;

        Collider[] _rangeChecksAbove = new Collider[8];
        int _hits;
        public bool CheckIfInVision () {
            _hits = Physics.OverlapSphereNonAlloc(transform.position, radius, _rangeChecksAbove, targetMask);
            //Collider[] rangeChecksBelow = Physics.OverlapSphere(new Vector3(0, offset, 0), radius, targetMask);

            //Debug.Log($"Transform on FOV component: {transform.position}");
            // Debug.Log($"Made collider array. Length is {rangeChecksAbove.Length}");
            if (_hits > 0) {
                // Debug.Log($"Checking for colliders");
                target = _rangeChecksAbove[0].transform;


                Vector3 directionToTarget = ((target.position + (Vector3.up*2)) - transform.position).normalized;

                // Debug.Log($"Target: {target}");
                if (Vector3.Angle(transform.forward, directionToTarget) < angle/2) {
                    // Debug.Log($"Checking angle");

                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}