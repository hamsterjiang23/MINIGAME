using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Helper
{
	public static string[] Split(this string line, char tempSeparator, string separator, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
	{
		return line.Replace(separator, new string(tempSeparator, 1)).Split(new char[1] { tempSeparator }, options);
	}

	public static string[] Split(this string line, char tempSeparator, string[] separators, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
	{
		for (int i = 0; i < separators.Length; i++)
		{
			line.Replace(separators[i], new string(tempSeparator, 1));
		}
		return line.Split(new char[1] { tempSeparator }, options);
	}

	public static IEnumerator FadeCanvasGroup(bool fadeIn, float fadeTimeInSec, CanvasGroup canvasGroup)
	{
		if (fadeIn)
		{
			canvasGroup.gameObject.SetActive(value: true);
		}
		float start = ((!fadeIn) ? 1 : 0);
		float end = (fadeIn ? 1 : 0);
		float lerpAmount = 0f;
		_ = Time.time;
		while (lerpAmount < 1.01f)
		{
			float num = Time.deltaTime / fadeTimeInSec;
			_ = canvasGroup.alpha;
			float alpha = Mathf.Lerp(start, end, lerpAmount);
			canvasGroup.alpha = alpha;
			lerpAmount += num;
			yield return null;
		}
		if (!fadeIn)
		{
			canvasGroup.gameObject.SetActive(value: false);
		}
	}

	public static IEnumerator FadeGraphic(bool fadeIn, float fadeTimeInSec, Graphic graphic)
	{
		float start = ((!fadeIn) ? 1 : 0);
		float end = (fadeIn ? 1 : 0);
		float lerpAmount = 0f;
		_ = Time.time;
		while (lerpAmount < 1.01f)
		{
			float num = Time.deltaTime / fadeTimeInSec;
			Color color = graphic.color;
			color.a = Mathf.Lerp(start, end, lerpAmount);
			graphic.color = color;
			lerpAmount += num;
			yield return null;
		}
	}

	public static IEnumerator FadeGraphic(bool fadeIn, float fadeTimeInSec, SpriteRenderer spriteRenderer)
	{
		float start = ((!fadeIn) ? 1 : 0);
		float end = (fadeIn ? 1 : 0);
		float lerpAmount = 0f;
		_ = Time.time;
		while (lerpAmount < 1.01f)
		{
			float num = Time.deltaTime / fadeTimeInSec;
			Color color = spriteRenderer.color;
			color.a = Mathf.Lerp(start, end, lerpAmount);
			spriteRenderer.color = color;
			lerpAmount += num;
			yield return null;
		}
	}

	public static IEnumerator CallActionAfterFade(bool fadeIn, float fadeTimeInSec, SpriteRenderer spriteRenderer, Action action)
	{
		float start = ((!fadeIn) ? 1 : 0);
		float end = (fadeIn ? 1 : 0);
		float lerpAmount = 0f;
		_ = Time.time;
		while (lerpAmount < 1.01f)
		{
			float num = Time.deltaTime / fadeTimeInSec;
			Color color = spriteRenderer.color;
			color.a = Mathf.Lerp(start, end, lerpAmount);
			spriteRenderer.color = color;
			lerpAmount += num;
			yield return null;
		}
		action();
	}

	public static IEnumerator AnimateStringText(TMP_Text animatedText, string text, float secsPerLetter)
	{
		string[] words = text.Split(' ');
		for (int i = 0; i < words.Length; i++)
		{
			string word = words[i];
			if (i != words.Length - 1)
			{
				word += " ";
			}
			string previousText = animatedText.text;
			float preferredHeight = animatedText.preferredHeight;
			animatedText.text += word;
			if (animatedText.preferredHeight > preferredHeight)
			{
				previousText += Environment.NewLine;
			}
			for (int j = 0; j < word.Length; j++)
			{
				animatedText.text = previousText + word.Substring(0, j + 1);
				yield return new WaitForSeconds(secsPerLetter);
			}
		}
		animatedText.text = text;
	}

	public static IEnumerator SelectGameObjectAfterFrame(GameObject selectObj)
	{
		yield return null;
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(selectObj);
	}

	public static GameObject GetSelectedGameObject()
	{
		return EventSystem.current.currentSelectedGameObject;
	}

	public static IEnumerator WaitAndCallAction(float seconds, Action action)
	{
		yield return new WaitForSeconds(seconds);
		action();
	}
}
