using System.Collections.Generic;
using Enigmaware.Entities;
using Enigmaware.World;
using UnityEngine;


//Sometimes I imagine Vegeta taking me as a malewife concubine and cry knowing that reality will never happen.  
public class MeleeAttack 
{
    public TeamComponent.Team Team;
    public HitboxGroupHandler HitboxGroupHandler;
     readonly List<HealthComponent> _ignoredHealthComponentList = new();
     readonly List<AttackResult> _attackResults = new();
    public DamageInfo DamageInfo;
     
    public struct AttackResult
    {
        public HealthComponent HealthComponent;
        public Hurtbox HitboxHandler;
        public Vector3 PushDirection;
        public Vector3 HitPosition;
    }
    
    public bool Filter(Hurtbox collider)
    {
        if (!collider.healthComponent)
        {
            Debug.Log($"Hitbox '{collider.gameObject.name}' has no health component!");
            return false;
        }
        if (HitboxGroupHandler.transform.IsChildOf(collider.healthComponent.transform))
        {
            return false;
        }
        if (this._ignoredHealthComponentList.Contains(collider.healthComponent))
        {
            return false;
        }

        TeamComponent.Team otherTeam = collider.healthComponent.Team;
        return otherTeam != Team;
    }
    
    
    /// <summary>
    /// Checks if the hitboxes are colliding with any hurtboxes.
    /// </summary>
    /// <returns>If true, there's something in </returns>
    public bool CheckHitboxes()
    {
        int count = 0;
        for (int i = 0; i < HitboxGroupHandler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = HitboxGroupHandler.hitboxes[i];
            Collider[] results = new Collider[32];

            Transform cachedTransform = hitboxVisualizer.transform;
            count += Physics.OverlapBoxNonAlloc(cachedTransform.position, cachedTransform.lossyScale * 0.5f, results, cachedTransform.rotation, LayerMask.GetMask("Hurtbox"));
        }

        return count > 0;
    }
    
    public List<Hitbox> CheckHitboxesSpecific()
    {
        List<Hitbox> hitboxes = new List<Hitbox>();
        for (int i = 0; i < HitboxGroupHandler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = HitboxGroupHandler.hitboxes[i];
            Collider[] results = new Collider[32];

            Transform cachedTransform = hitboxVisualizer.transform;
            if (Physics.OverlapBoxNonAlloc(cachedTransform.position, cachedTransform.lossyScale * 0.5f, results,
                    cachedTransform.rotation, LayerMask.GetMask("Hurtbox")) > 0)
            {
                hitboxes.Add(hitboxVisualizer);
            }
        }
        return hitboxes;
    }
    
    /// <summary>
    /// Checks every hitbox for collisions with hurtboxes and stores the results.
    /// </summary>
    /// <returns></returns>
    public List<AttackResult> Hit()
    {
        for (int i = 0; i < HitboxGroupHandler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = HitboxGroupHandler.hitboxes[i];
            Collider[] results = new Collider[32];

            Transform cachedTransform = hitboxVisualizer.transform;
            int count = Physics.OverlapBoxNonAlloc(cachedTransform.position, cachedTransform.lossyScale * 0.5f, results, cachedTransform.rotation, LayerMask.GetMask("Hurtbox"));
            
            // go through results
            for (int j = 0; j < count; j++)
            {
                Debug.Log(results[j]);
                Hurtbox hitbox= results[j].GetComponent<Hurtbox>();
                
                if (hitbox != null && Filter(hitbox))
                {
                    _ignoredHealthComponentList.Add(hitbox.healthComponent);
                    Vector3 position = hitbox.transform.position;
                    WorldBounder.DebugBounds(results[j].bounds, Color.green, 10);

                    // 
                    _attackResults.Add(new AttackResult
                    {
                        HealthComponent = hitbox.healthComponent,
                        HitboxHandler = hitbox,
                        PushDirection =  (position - cachedTransform.position).normalized,
                        HitPosition = position,
                    });
                    
                }
            }
        }
        foreach (var result in _attackResults) {
            result.HealthComponent.TakeDamage(DamageInfo);
        }
        return _attackResults;
    }
}