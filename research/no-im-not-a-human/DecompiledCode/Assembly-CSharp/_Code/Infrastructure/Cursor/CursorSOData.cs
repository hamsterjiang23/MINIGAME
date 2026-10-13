using UnityEngine;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.Cursor;

[CreateAssetMenu(menuName = "Cursor Data")]
[TabsNames(new string[] { "Sprites" })]
public sealed class CursorSOData : DataClass
{
	[TabIndex(0)]
	[SerializeField]
	private CursorTextureByTypeData[] _textureByType;

	public CursorTextureByTypeData[] TextureByType => _textureByType;
}
