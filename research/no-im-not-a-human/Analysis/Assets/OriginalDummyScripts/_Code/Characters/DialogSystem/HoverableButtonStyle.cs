using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Characters.DialogSystem
{
	[CreateAssetMenu(menuName = "UI/DialogButtonStyle")]
	public sealed class HoverableButtonStyle : ScriptableObject
	{
		[field: SerializeField]
		public EDialogButtonStyle Style { get; private set; }

		[field: SerializeField]
		public AnimationData BaseImage { get; private set; }

		[field: SerializeField]
		public AnimationData HoveredImage { get; private set; }

		[field: SerializeField]
		public Color BaseColor { get; private set; }

		[field: SerializeField]
		public Color HoveredColor { get; private set; }

		[Tooltip("Left, Top, Right, Bottom")]
		[field: SerializeField]
		public Vector4 BasePaddings { get; private set; }

		[Tooltip("Left, Top, Right, Bottom")]
		[field: SerializeField]
		public Vector4 HoveredPaddings { get; private set; }
	}
}
