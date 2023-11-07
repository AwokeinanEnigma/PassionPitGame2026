/// <summary>
///     Called when something is attempting to damage the GO's health component.
///     Setting the 'rejected' field of 'damageInfo' to false will prevent the damage from being dealt.
/// </summary>
public interface IOnIncomingDamage {
	void OnIncomingDamage (ref DamageInfo info);
}
