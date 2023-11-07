/// <summary>
///     Called on the GO's health component immediately after it has taken damage and its health has been reduced.
///     Will still be called on the killing blow.
/// </summary>
public interface IOnTakeDamage {
	void OnTakeDamage (DamageReport damageReport);
}
