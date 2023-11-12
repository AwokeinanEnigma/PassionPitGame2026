#region

using Enigmaware.Entities;
using Enigmaware.World;
using PassionPitGame;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion
public class ExplosionAttack {

	Collider[] _colliders;
	readonly List<Result> _results = new();
	RaycastHit[] _hits;
	
	/// <summary>
	/// If this is true, the explosion will hit the environment.
	/// </summary>
	public bool HitEnvironment = false;
	/// <summary>
	/// If this is true, the explosion will hit everyone, regardless of team.
	/// </summary>
	public bool HitEveryone = false;
	public DamageInfo DamageInfo;
	public LayerMask LayerMask;

	/// <summary>
	/// How many hits this explosion can have.
	/// </summary>
	public int MaximumHits = 32;
	/// <summary>
	/// The position of the explosion.
	/// </summary>
	public Vector3 Position;
	/// <summary>
	/// How far the explosion will reach.
	/// </summary>
	public float Radius;
	/// <summary>
	/// The team that this explosion is on.
	/// </summary>
	public TeamComponent.Team Team;
	/// <summary>
	/// If this is enabled, the a raycast will be performed to ensure there's no walls between the explosion and the target.
	/// </summary>
	public bool UseAccuracy = false;
	/// <summary>
	/// If this is enabled, the explosion will be visualized via a sphere.
	/// </summary>
	public bool Visualize = false;
	/// <summary>
	/// Executes the explosion.
	/// </summary>
	/// <returns>A list of every entity it hit.</returns>
	public List<Result> Fire () {
		_colliders = new Collider[MaximumHits];
		_hits = new RaycastHit[MaximumHits];
		int count = Physics.OverlapSphereNonAlloc(Position, Radius, _colliders, LayerMask);
		for (int i = 0; i < count; i++) {
			var collider = _colliders[i];
			Hurtbox hurtbox = collider.GetComponent<Hurtbox>();

			// if it's not a hurtbox, and we're not hitting everything, skip it
			if (hurtbox == null && !HitEnvironment) continue;

			HealthComponent healthComponent = hurtbox.healthComponent;
			// if it's on our team, and we're not hitting everyone, skip it
			if (Team == healthComponent.Team && !HitEveryone) {
				continue;
			}

			WorldBounder.DebugBounds(collider.bounds, Color.green, 10);
			
			if (UseAccuracy) {
				Debug.DrawRay(Position, collider.transform.position - Position, Color.yellow, 104);
				if (Physics.RaycastNonAlloc(new Ray(Position, collider.transform.position - Position), _hits, Vector3.Distance(Position, collider.transform.position), LayerMask.GetMask("Environment")) == 0) {
					Transform transform = collider.transform;
					var result = new Result {
						Collider = collider,
						HitPoint = collider.ClosestPoint(Position),
						HitNormal = _hits[0].normal,
						HitDirection = Position - transform.position,
						HitDistance = _hits[0].distance,
						Hurtbox = hurtbox,
						HealthComponent = healthComponent,
						Team = healthComponent.Team,

					};
					_results.Add(result);
				}
				Debug.LogError("prevented");
			} else {
				Transform transform;
				var result = new Result {
					Collider = collider,
					HitPoint = collider.ClosestPoint(Position),
					HitNormal = (transform = collider.transform).position - Position,
					HitDirection = Position - transform.position,
					HitDistance = Vector3.Distance(Position, transform.position),
					Hurtbox = hurtbox,
					HealthComponent = healthComponent,
					Team = healthComponent.Team,
					

				};
				_results.Add(result);
			}
		}
		
		foreach (var result in _results) {
			result.HealthComponent.TakeDamage(DamageInfo);
		}
		
		if (Visualize) {
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			obj.transform.position = Position;
			obj.transform.localScale = Vector3.one*Radius*2;
			obj.AddComponent<DestructionTimer>().time = 5f;
			obj.GetComponent<Renderer>().material = Team == TeamComponent.Team.Player ? Resources.Load<Material>("UModeler_Grid_URP 2") : Resources.Load<Material>("UModeler_Grid_URP 1");
		}
	return _results;
	}
	public struct Result {
		public Collider Collider;
		public Vector3 HitPoint;
		public Vector3 HitNormal;
		public Vector3 HitDirection;
		public float HitDistance;
		public Hurtbox Hurtbox;
		public HealthComponent HealthComponent;
		public TeamComponent.Team Team;
	}
}
