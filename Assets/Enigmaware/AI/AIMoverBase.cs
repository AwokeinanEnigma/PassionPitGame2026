using Enigmaware.Motor;
using PassionPitGame;
using System;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

namespace Enigmaware.AI
{
    public abstract class AIMoverBase : MonoBehaviour
    {
        [Header("This is where the AI is moving towards")]
        public Vector3 destination;
        public Vector3 WishDirection;

        #region Components
        
        public PassionPitGame.Motor aiMotor;
        public Seeker seeker;
        public RVOController rvoController;
        
        #endregion

        #region Paths
        
        public abstract bool hasPath { get;  }
        public abstract bool pathPending { get; }
        public  abstract bool reachedDestination { get; }

        #endregion

        public void Awake()
        {
            //aiMotor = GetComponent<PassionPitGame.Motor>();
            seeker = GetComponent<Seeker>();
            rvoController = GetComponent<RVOController>();
        }
        
        public Action onSearchPath { get; set; }
        public virtual void SearchPath()
        {
            onSearchPath?.Invoke();
        }

        public abstract void Move();

        public Action onTargetReached { get; set; }
        public virtual void OnTargetReached()
        {
            onTargetReached?.Invoke();
        }
    }
}