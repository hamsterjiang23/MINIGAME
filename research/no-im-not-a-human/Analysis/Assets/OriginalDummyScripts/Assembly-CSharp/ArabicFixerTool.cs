using System;
using System.Collections.Generic;
using System.Text;

internal class ArabicFixerTool
{
	internal static bool showTashkeel = true;

	internal static bool combineTashkeel = true;

	internal static bool useHinduNumbers = false;

	internal static StringBuilder internalStringBuilder = new StringBuilder();

	internal static void RemoveTashkeel(ref string str, out List<TashkeelLocation> tashkeelLocation)
	{
		tashkeelLocation = new List<TashkeelLocation>();
		int lastSplitIndex = 0;
		internalStringBuilder.Clear();
		internalStringBuilder.EnsureCapacity(str.Length);
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '\u064b')
			{
				tashkeelLocation.Add(new TashkeelLocation('\u064b', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == '\u064c')
			{
				tashkeelLocation.Add(new TashkeelLocation('\u064c', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == '\u064d')
			{
				tashkeelLocation.Add(new TashkeelLocation('\u064d', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == '\u064e')
			{
				if (num > 0 && combineTashkeel && tashkeelLocation[num - 1].tashkeel == '\u0651')
				{
					tashkeelLocation[num - 1].tashkeel = 'ﱠ';
					IncrementSB(ref str, i);
				}
				else
				{
					tashkeelLocation.Add(new TashkeelLocation('\u064e', i));
					num++;
					IncrementSB(ref str, i);
				}
			}
			else if (str[i] == '\u064f')
			{
				if (num > 0 && combineTashkeel && tashkeelLocation[num - 1].tashkeel == '\u0651')
				{
					tashkeelLocation[num - 1].tashkeel = 'ﱡ';
					IncrementSB(ref str, i);
				}
				else
				{
					tashkeelLocation.Add(new TashkeelLocation('\u064f', i));
					num++;
					IncrementSB(ref str, i);
				}
			}
			else if (str[i] == '\u0650')
			{
				if (num > 0 && combineTashkeel && tashkeelLocation[num - 1].tashkeel == '\u0651')
				{
					tashkeelLocation[num - 1].tashkeel = 'ﱢ';
					IncrementSB(ref str, i);
				}
				else
				{
					tashkeelLocation.Add(new TashkeelLocation('\u0650', i));
					num++;
					IncrementSB(ref str, i);
				}
			}
			else if (str[i] == '\u0651')
			{
				if (num > 0 && combineTashkeel)
				{
					if (tashkeelLocation[num - 1].tashkeel == '\u064e')
					{
						tashkeelLocation[num - 1].tashkeel = 'ﱠ';
						IncrementSB(ref str, i);
						continue;
					}
					if (tashkeelLocation[num - 1].tashkeel == '\u064f')
					{
						tashkeelLocation[num - 1].tashkeel = 'ﱡ';
						IncrementSB(ref str, i);
						continue;
					}
					if (tashkeelLocation[num - 1].tashkeel == '\u0650')
					{
						tashkeelLocation[num - 1].tashkeel = 'ﱢ';
						IncrementSB(ref str, i);
						continue;
					}
				}
				tashkeelLocation.Add(new TashkeelLocation('\u0651', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == '\u0652')
			{
				tashkeelLocation.Add(new TashkeelLocation('\u0652', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == '\u0653')
			{
				tashkeelLocation.Add(new TashkeelLocation('\u0653', i));
				num++;
				IncrementSB(ref str, i);
			}
			else if (str[i] == 'ﱠ')
			{
				IncrementSB(ref str, i);
			}
			else if (str[i] == 'ﱡ')
			{
				IncrementSB(ref str, i);
			}
			else if (str[i] == 'ﱢ')
			{
				IncrementSB(ref str, i);
			}
		}
		if (lastSplitIndex != 0)
		{
			IncrementSB(ref str, str.Length);
			str = internalStringBuilder.ToString();
		}
		void IncrementSB(ref string reference, int num2)
		{
			if (num2 - lastSplitIndex > 0)
			{
				internalStringBuilder.Append(reference, lastSplitIndex, num2 - lastSplitIndex);
			}
			lastSplitIndex = num2 + 1;
		}
	}

	internal static void ReturnTashkeel(ref char[] letters, List<TashkeelLocation> tashkeelLocation)
	{
		Array.Resize(ref letters, letters.Length + tashkeelLocation.Count);
		for (int i = 0; i < tashkeelLocation.Count; i++)
		{
			TashkeelLocation tashkeelLocation2 = tashkeelLocation[i];
			for (int num = letters.Length - 1; num > tashkeelLocation2.position; num--)
			{
				letters[num] = letters[num - 1];
			}
			letters[tashkeelLocation2.position] = tashkeelLocation2.tashkeel;
		}
	}

	internal static string FixLine(string str)
	{
		RemoveTashkeel(ref str, out var tashkeelLocation);
		char[] array = new char[str.Length];
		char[] letters = str.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (char)ArabicTable.ArabicMapper.Convert(str[i]);
		}
		for (int j = 0; j < array.Length; j++)
		{
			bool flag = false;
			if (array[j] == 'ﻝ' && j < array.Length - 1)
			{
				if (array[j + 1] == 'ﺇ')
				{
					array[j] = 'ﻷ';
					letters[j + 1] = '\uffff';
					flag = true;
				}
				else if (array[j + 1] == 'ﺍ')
				{
					array[j] = 'ﻹ';
					letters[j + 1] = '\uffff';
					flag = true;
				}
				else if (array[j + 1] == 'ﺃ')
				{
					array[j] = 'ﻵ';
					letters[j + 1] = '\uffff';
					flag = true;
				}
				else if (array[j + 1] == 'ﺁ')
				{
					array[j] = 'ﻳ';
					letters[j + 1] = '\uffff';
					flag = true;
				}
			}
			if (!IsIgnoredCharacter(array[j]))
			{
				if (IsMiddleLetter(array, j))
				{
					letters[j] = (char)(array[j] + 3);
				}
				else if (IsFinishingLetter(array, j))
				{
					letters[j] = (char)(array[j] + 1);
				}
				else if (IsLeadingLetter(array, j))
				{
					letters[j] = (char)(array[j] + 2);
				}
			}
			if (flag)
			{
				j++;
			}
			if (useHinduNumbers)
			{
				letters[j] = (char)HandleInduNumber(array[j], letters[j]);
			}
		}
		if (showTashkeel && tashkeelLocation.Count > 0)
		{
			ReturnTashkeel(ref letters, tashkeelLocation);
		}
		internalStringBuilder.Clear();
		internalStringBuilder.EnsureCapacity(letters.Length);
		List<char> numberList = null;
		for (int num = letters.Length - 1; num >= 0; num--)
		{
			if (char.IsPunctuation(letters[num]) && num > 0 && num < letters.Length - 1 && (char.IsPunctuation(letters[num - 1]) || char.IsPunctuation(letters[num + 1])))
			{
				if (letters[num] == '(')
				{
					internalStringBuilder.Append(')');
				}
				else if (letters[num] == ')')
				{
					internalStringBuilder.Append('(');
				}
				else if (letters[num] == '<')
				{
					internalStringBuilder.Append('>');
				}
				else if (letters[num] == '>')
				{
					internalStringBuilder.Append('<');
				}
				else if (letters[num] == '[')
				{
					internalStringBuilder.Append(']');
				}
				else if (letters[num] == ']')
				{
					internalStringBuilder.Append('[');
				}
				else if (letters[num] != '\uffff')
				{
					internalStringBuilder.Append(letters[num]);
				}
			}
			else if (letters[num] == ' ' && num > 0 && num < letters.Length - 1 && (char.IsLower(letters[num - 1]) || char.IsUpper(letters[num - 1]) || char.IsNumber(letters[num - 1])) && (char.IsLower(letters[num + 1]) || char.IsUpper(letters[num + 1]) || char.IsNumber(letters[num + 1])))
			{
				AddNumber(letters[num]);
			}
			else if (char.IsNumber(letters[num]) || char.IsLower(letters[num]) || char.IsUpper(letters[num]) || char.IsSymbol(letters[num]) || char.IsPunctuation(letters[num]))
			{
				if (letters[num] == '(')
				{
					AddNumber(')');
				}
				else if (letters[num] == ')')
				{
					AddNumber('(');
				}
				else if (letters[num] == '<')
				{
					AddNumber('>');
				}
				else if (letters[num] == '>')
				{
					AddNumber('<');
				}
				else if (letters[num] == '[')
				{
					internalStringBuilder.Append(']');
				}
				else if (letters[num] == ']')
				{
					internalStringBuilder.Append('[');
				}
				else
				{
					AddNumber(letters[num]);
				}
			}
			else if ((letters[num] >= '\ud800' && letters[num] <= '\udbff') || (letters[num] >= '\udc00' && letters[num] <= '\udfff'))
			{
				AddNumber(letters[num]);
			}
			else
			{
				AppendNumbers();
				if (letters[num] != '\uffff')
				{
					internalStringBuilder.Append(letters[num]);
				}
			}
		}
		AppendNumbers();
		return internalStringBuilder.ToString();
		void AddNumber(char value)
		{
			if (numberList == null)
			{
				numberList = new List<char>();
			}
			numberList.Add(value);
		}
		void AppendNumbers()
		{
			if (numberList != null && numberList.Count > 0)
			{
				for (int k = 0; k < numberList.Count; k++)
				{
					internalStringBuilder.Append(numberList[numberList.Count - 1 - k]);
				}
				numberList.Clear();
			}
		}
	}

	internal static ushort HandleInduNumber(ushort letterOrigin, ushort letterFinal)
	{
		return letterOrigin switch
		{
			48 => (ushort)1632, 
			49 => (ushort)1633, 
			50 => (ushort)1634, 
			51 => (ushort)1635, 
			52 => (ushort)1636, 
			53 => (ushort)1637, 
			54 => (ushort)1638, 
			55 => (ushort)1639, 
			56 => (ushort)1640, 
			57 => (ushort)1641, 
			_ => letterFinal, 
		};
	}

	internal static bool IsIgnoredCharacter(char ch)
	{
		bool flag = char.IsPunctuation(ch);
		bool flag2 = char.IsNumber(ch);
		bool flag3 = char.IsLower(ch);
		bool flag4 = char.IsUpper(ch);
		bool flag5 = char.IsSymbol(ch);
		bool flag6 = ch == 'ﭖ' || ch == 'ﭺ' || ch == 'ﮊ' || ch == 'ﮒ' || ch == 'ﮎ';
		bool flag7 = ((ch <= '\ufeff' && ch >= 'ﹰ') | flag6) || ch == 'ﯼ';
		if (!(flag | flag2 | flag3 | flag4 | flag5) && flag7 && ch != 'a' && ch != '>' && ch != '<')
		{
			return ch == '؛';
		}
		return true;
	}

	internal static bool IsLeadingLetter(char[] letters, int index)
	{
		bool num = index == 0 || letters[index - 1] == ' ' || letters[index - 1] == '*' || letters[index - 1] == 'A' || char.IsPunctuation(letters[index - 1]) || letters[index - 1] == '>' || letters[index - 1] == '<' || letters[index - 1] == 'ﺍ' || letters[index - 1] == 'ﺩ' || letters[index - 1] == 'ﺫ' || letters[index - 1] == 'ﺭ' || letters[index - 1] == 'ﺯ' || letters[index - 1] == 'ﮊ' || letters[index - 1] == 'ﻭ' || letters[index - 1] == 'ﺁ' || letters[index - 1] == 'ﺃ' || letters[index - 1] == 'ﺀ' || letters[index - 1] == 'ﺇ' || letters[index - 1] == 'ﺅ';
		bool flag = letters[index] != ' ' && letters[index] != 'ﺩ' && letters[index] != 'ﺫ' && letters[index] != 'ﺭ' && letters[index] != 'ﺯ' && letters[index] != 'ﮊ' && letters[index] != 'ﺍ' && letters[index] != 'ﺃ' && letters[index] != 'ﺇ' && letters[index] != 'ﺁ' && letters[index] != 'ﺅ' && letters[index] != 'ﻭ' && letters[index] != 'ﺀ';
		bool flag2 = index < letters.Length - 1 && letters[index + 1] != ' ' && letters[index + 1] != '\n' && letters[index + 1] != '\r' && !char.IsPunctuation(letters[index + 1]) && !char.IsNumber(letters[index + 1]) && !char.IsSymbol(letters[index + 1]) && !char.IsLower(letters[index + 1]) && !char.IsUpper(letters[index + 1]) && letters[index + 1] != 'ﺀ';
		return num & flag & flag2;
	}

	internal static bool IsFinishingLetter(char[] letters, int index)
	{
		bool num = index != 0 && letters[index - 1] != ' ' && letters[index - 1] != 'ﺩ' && letters[index - 1] != 'ﺫ' && letters[index - 1] != 'ﺭ' && letters[index - 1] != 'ﺯ' && letters[index - 1] != 'ﮊ' && letters[index - 1] != 'ﻭ' && letters[index - 1] != 'ﺍ' && letters[index - 1] != 'ﺁ' && letters[index - 1] != 'ﺃ' && letters[index - 1] != 'ﺇ' && letters[index - 1] != 'ﺅ' && letters[index - 1] != 'ﺀ' && !char.IsPunctuation(letters[index - 1]) && !char.IsSymbol(letters[index - 1]) && letters[index - 1] != '>' && letters[index - 1] != '<';
		bool flag = letters[index] != ' ' && letters[index] != 'ﺀ';
		return num & flag;
	}

	internal static bool IsMiddleLetter(char[] letters, int index)
	{
		bool flag = index != 0 && letters[index] != 'ﺍ' && letters[index] != 'ﺩ' && letters[index] != 'ﺫ' && letters[index] != 'ﺭ' && letters[index] != 'ﺯ' && letters[index] != 'ﮊ' && letters[index] != 'ﻭ' && letters[index] != 'ﺁ' && letters[index] != 'ﺃ' && letters[index] != 'ﺇ' && letters[index] != 'ﺅ' && letters[index] != 'ﺀ';
		bool flag2 = index != 0 && letters[index - 1] != 'ﺍ' && letters[index - 1] != 'ﺩ' && letters[index - 1] != 'ﺫ' && letters[index - 1] != 'ﺭ' && letters[index - 1] != 'ﺯ' && letters[index - 1] != 'ﮊ' && letters[index - 1] != 'ﻭ' && letters[index - 1] != 'ﺁ' && letters[index - 1] != 'ﺃ' && letters[index - 1] != 'ﺇ' && letters[index - 1] != 'ﺅ' && letters[index - 1] != 'ﺀ' && !char.IsPunctuation(letters[index - 1]) && letters[index - 1] != '>' && letters[index - 1] != '<' && letters[index - 1] != ' ' && letters[index - 1] != '*';
		if ((index < letters.Length - 1 && letters[index + 1] != ' ' && letters[index + 1] != '\r' && letters[index + 1] != 'ﺀ' && !char.IsNumber(letters[index + 1]) && !char.IsSymbol(letters[index + 1]) && !char.IsPunctuation(letters[index + 1])) & flag2 & flag)
		{
			return !char.IsPunctuation(letters[index + 1]);
		}
		return false;
	}
}
