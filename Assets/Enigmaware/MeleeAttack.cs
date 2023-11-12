using System.Collections.Generic;
using Enigmaware.Entities;
using Enigmaware.World;
using UnityEngine;


//Sometimes I imagine Vegeta taking me as a malewife concubine and cry knowing that reality will never happen.  
public class MeleeAttack 
{
    public TeamComponent.Team team;
    public HitboxGroupHandler handler;
    private readonly List<HealthComponent> ignoredHealthComponentList = new();
    private readonly List<AttackResult> attackResults = new();
    
    public struct AttackResult
    {
        public HealthComponent healthComponent;
        public Hurtbox hitboxHandler;
        public Vector3 pushDirection;
        public Vector3 hitPosition;
    }
    
    public bool Filter(Hurtbox collider)
    {
        if (!collider.healthComponent)
        {
            Debug.Log($"Hitbox '{collider.gameObject.name}' has no health component!");
            return false;
        }
        if (handler.transform.IsChildOf(collider.healthComponent.transform))
        {
            return false;
        }
        if (this.ignoredHealthComponentList.Contains(collider.healthComponent))
        {
            return false;
        }

        TeamComponent.Team otherTeam = collider.healthComponent.Team;
        return otherTeam != team;
    }
    
    
    /// <summary>
    /// Checks if the hitboxes are colliding with any hurtboxes.
    /// </summary>
    /// <returns>If true, there's something in </returns>
    public bool CheckHitboxes()
    {
        int count = 0;
        for (int i = 0; i < handler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = handler.hitboxes[i];
            Collider[] results = new Collider[32];

            Transform cachedTransform = hitboxVisualizer.transform;
            count += Physics.OverlapBoxNonAlloc(cachedTransform.position, cachedTransform.lossyScale * 0.5f, results, cachedTransform.rotation, LayerMask.GetMask("Hurtbox"));
        }

        return count > 0;
    }
    
    public List<Hitbox> CheckHitboxesSpecific()
    {
        List<Hitbox> hitboxes = new List<Hitbox>();
        for (int i = 0; i < handler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = handler.hitboxes[i];
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
        for (int i = 0; i < handler.hitboxes.Length; i++)
        {
            Hitbox hitboxVisualizer = handler.hitboxes[i];
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
                    ignoredHealthComponentList.Add(hitbox.healthComponent);
                    Vector3 position = hitbox.transform.position;
                    WorldBounder.DebugBounds(results[j].bounds, Color.green, 10);

                    // 
                    attackResults.Add(new AttackResult
                    {
                        healthComponent = hitbox.healthComponent,
                        hitboxHandler = hitbox,
                        pushDirection =  (position - cachedTransform.position).normalized,
                        hitPosition = position,
                    });
                    
                }
            }
        }
        return attackResults;
    }
}