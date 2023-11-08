using EntityStates;
namespace PassionPitGame {
	public abstract class AUXState : EntityState {
		public CardDeck CardDeck;
		public abstract void OnClick();

		public override bool CanBeInterrupted () { return true; }
	}
}
