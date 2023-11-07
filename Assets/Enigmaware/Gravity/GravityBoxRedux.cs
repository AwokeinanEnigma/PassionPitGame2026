using UnityEngine;

namespace Enigmaware.Gravity
{

    public class GravityBoxRedux : GravitySource
    {

        [SerializeField] float gravity = 9.81f;

        [SerializeField] Vector3 boundaryDistance = Vector3.one;

        public override Vector3 GetGravity(Vector3 position)
        {
            Vector3 vector = Vector3.zero;
            int outside = 0; 
            if (position.x < -boundaryDistance.x)
            {
                vector.x = -boundaryDistance.x - position.x;
                outside = 1;
            }

            if (position.y < -boundaryDistance.y)
            {
                vector.y = -boundaryDistance.y - position.y;
                outside += 1;
            }
            
            if (position.z < -boundaryDistance.z)
            {
                vector.z = -boundaryDistance.z - position.z;
                outside += 1;
            }

            //Debug.Log($"Outside is {outside}");
            if (outside < 0)
            {
                Vector3 up = transform.up;
                float distance = Vector3.Dot(up, position - transform.position);
                float g = -gravity;
                g *= 1f - distance;
                Debug.Log(g * up);
            
                return g * up;
            }

            return Vector3.zero;
        }
		

        void Awake()
        {
            OnValidate();
        }

        void OnValidate()
        {
            boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix =
                Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Vector3 size;

            Gizmos.color = Color.cyan;
            size.x = boundaryDistance.x;//- innerFalloffDistance);
            size.y = boundaryDistance.y ;
            size.z = boundaryDistance.z;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}   