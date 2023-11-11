using System;
using System.Collections;
using System.Collections.Generic;
using Enigmaware.AI;
using Enigmaware.Entities;
using Enigmaware.General;
using Enigmaware.Motor;
using Enigmaware.Projectiles;
using PassionPitGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

namespace EntityStates.AI
{
    public class ConceptConstruct : EntityState
    {
        public Transform player;
        public AdvancedAIMover mover;
        public AIMotor motor;
        public Transform child;
        public LoS los;
        
        private float _timer;
        private float _random;
        private float _shootTimer;
        public RotateTowardsDirection rotator;
        
        public enum HidekiRunnerState
        {
            Run,
            Shoot,
            Melee
        }
        public HidekiRunnerState state = HidekiRunnerState.Run;

        public override void OnEnter()
        {
            base.OnEnter();
            _random = Random.Range(1, 2);
            _shootTimer = 2.5f;

            child = stateMachine.GetComponent<TransformDictionary>().FindTransform("ProjectilePoint");
            los = GetComponentInChildren<LoS>();
            
            // see grown men cry
            player = GameObject.Find("PlayerObject").GetComponentInChildren<Camera>().transform;
            mover = stateMachine.GetComponentInParent<AdvancedAIMover>();
            motor = stateMachine.GetComponentInParent<AIMotor>();
            rotator = stateMachine.GetComponentInChildren<RotateTowardsDirection>();

            if (mover != null)
                mover.onSearchPath += Redirect;
            
        }

        public void Redirect()
        {
            mover.destination = player.position;
        }
        
        public override void Update()
        {
            base.Update();
            
            mover.destination = player.position;
            if (!mover.reachedDestination) {
                rotator.Direction = Flatten(mover.WishDirection);
            }
            Debug.Log(mover.reachedDestination);

            // if (mover.reachedDestination && state != HidekiRunnerState.Shoot && state != HidekiRunnerState.Melee)
            {
             //   rotator.Direction  = player.position - transform.position;
               // stateMachine.StartCoroutine(Melee());
            }

            // don't move or search unless we're in the air
            // save performance
            mover.canSearch = motor.IsGrounded;
//            Debug.Log(mover.canSearch);

            _timer += Time.deltaTime;
            if (_timer > _shootTimer && state != HidekiRunnerState.Shoot && state != HidekiRunnerState.Melee && los.CheckLOS() && motor.IsGrounded)
            {
                var position = child.position;
                ProjectileInfo info = new ProjectileInfo()
                {
                    projectilePrefab = Resources.Load<GameObject>("Prefabs/LiterallyMe"),
                    owner = this.gameObject,
                    moveDir = (player.position - position).normalized,
                    position = position,
                    team = TeamComponent.Team.Enemy,
                    damageInfo = new DamageInfo()
                    {
                        Attacker = base.gameObject,
                        Damage = 1,
                        Force = Vector3.up * 20,
                        Inflictor = null,
                    }
                };
                ProjectileController.LaunchProjectile(info);
                               //motor.ForceMovementType(AIMotor.MovementType.Ground);
                state = HidekiRunnerState.Run;
                _timer = 0;
            }
        }

        public Vector3 Flatten(Vector3 vector)
        {
            vector.y = 0;
            return vector;
        }
        

        
        
        public IEnumerator Melee()
        {
            state = HidekiRunnerState.Melee;
            motor.GroundMovementSpeed = 0;
            yield return new WaitForSeconds(0.6f);

            MeleeAttack attack = new MeleeAttack()
            {
                handler = Array.Find(base.transform.GetComponentsInChildren<HitboxGroupHandler>(),
                    element => element.name == "PickMe"),
                team = TeamComponent.Team.Enemy,
            };
            List<MeleeAttack.AttackResult> reesults = attack.Hit();
            foreach (MeleeAttack.AttackResult result in reesults)
            {
                CharacterMotor motor = result.healthComponent.GetComponent<CharacterMotor>();
                result.healthComponent.GetComponent<CharacterMotor>().KMotor.ForceUnground();
                Vector3 vector3 = result.pushDirection * 15;
                vector3.y += 50;
            }
            
            yield return new WaitForSeconds(1.357f);
            motor.GroundMovementSpeed = 7;    
            //motor.ForceMovementType(AIMotor.MovementType.Ground);
            state = HidekiRunnerState.Run;
            _timer = 0;
        }
        

        public override void OnExit()
        {
            base.OnExit();
            if (mover != null) mover.onSearchPath -= Redirect;
        }
    }
}