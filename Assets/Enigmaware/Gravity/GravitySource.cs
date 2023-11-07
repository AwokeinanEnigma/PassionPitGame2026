using UnityEngine;

namespace Enigmaware.Gravity
{

    public class GravitySource : MonoBehaviour
    {

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        void OnEnable()
        {
            GravityFinder.Register(this);
        }

        void OnDisable()
        {
            GravityFinder.Unregister(this);
        }
    }
}