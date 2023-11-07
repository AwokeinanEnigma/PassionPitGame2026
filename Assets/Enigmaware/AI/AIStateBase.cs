using Enigmaware.AI;
using Enigmaware.Motor;

namespace EntityStates.AI
{
    public class AIStateBase : EntityState
    {
        public AIMoverBase mover;
        public KinematicNPCMotor aiMotor;
        
        public override void OnEnter()
        {
            base.OnEnter();
            mover = stateMachine.GetComponentInParent<AIMoverBase>();
            aiMotor = stateMachine.GetComponentInParent<KinematicNPCMotor>();
            
            mover.onTargetReached += OnTargetReached;
            mover.onSearchPath += SearchPath;
        }

        public override void Update()
        {
            base.Update();
            SetDestination();
            if (!mover.reachedDestination)
            {
                WhileOnPath();
            }
        }
        
        public virtual void SetDestination()
        {
            //mover.destination = destination;
        }
        
        public virtual void WhileOnPath()
        {
            
        }
        
        public virtual void OnTargetReached()
        {
        }

        public virtual void SearchPath()
        {
        }
        
        public override void OnExit()
        {
            base.OnExit();
            
            mover.onTargetReached -= OnTargetReached;
            mover.onSearchPath -= SearchPath;

        }
    }
}