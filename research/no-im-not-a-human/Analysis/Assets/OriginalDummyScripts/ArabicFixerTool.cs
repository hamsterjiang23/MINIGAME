using System.Collections.Generic;
using System.Text;

internal class ArabicFixerTool
{
	internal static bool showTashkeel;

	internal static bool combineTashkeel;

	internal static bool useHinduNumbers;

	internal static StringBuilder internalStringBuilder;

	internal static void RemoveTashkeel(ref string str, out List<TashkeelLocation> tashkeelLocation)
	{
		tashkeelLocation = null;
	}

	internal static void ReturnTashkeel(ref char[] letters, List<TashkeelLocation> tashkeelLocation)
	{
	}

	internal static string FixLine(string str)
	{
		return null;
	}

	internal static ushort HandleInduNumber(ushort letterOrigin, ushort letterFinal)
	{
		return 0;
	}

	internal static bool IsIgnoredCharacter(char ch)
	{
		return false;
	}

	internal static bool IsLeadingLetter(char[] letters, int index)
	{
		return false;
	}

	internal static bool IsFinishingLetter(char[] letters, int index)
	{
		return false;
	}

	internal static bool IsMiddleLetter(char[] letters, int index)
	{
		return false;
	}
}
