using _Code.Characters;

namespace _Scripts.Services.DataModel.Models.Dialogs
{
	public interface IDialogPrefsDataHandler
	{
		void LoadData();

		void AddCharacterWithWhoTalkedToday(ECharacterType characterType);

		void ApplyCharacterTalks();

		int GetTalksCount(ECharacterType characterType);

		void SetToLastTalk(ECharacterType character);

		void AddMaxTalksCount(ECharacterType character, int count);
	}
}
