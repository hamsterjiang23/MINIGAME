using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Code.Characters.DialogSystem
{
	public interface IDialogUI
	{
		bool HasDialogNow { get; }

		void HideHands();

		void ShowHands(Sprite characterHandsSprite);

		UniTaskVoid Screamer();

		void SetExtraCharacter(CharacterSOData character);

		void ClearData();

		void InitAnswers(DialogAnswer[] answers);

		void ShowButtons();

		void HideButtons();

		void InitText(string text, CharacterSOData characterSoData, EDialogEmotionState emotionState);

		void Show();

		void Hide();

		void ShowTeeth(Sprite characterTeethSprite);

		void ShowEye(Sprite characterEyeSprite);

		void HideTeeth();

		void HideEye();
	}
}
