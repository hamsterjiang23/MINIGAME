using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Menues;

namespace _Code.Characters;

public sealed class BinPackager : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private TestCheck _testCheck;

	private readonly Queue<KeyCode> _lastKeys = new Queue<KeyCode>(5);

	private bool _is;

	private void Update()
	{
		_is = false;
		for (KeyCode keyCode = KeyCode.A; keyCode <= KeyCode.Z; keyCode++)
		{
			if (Input.GetKeyDown(keyCode))
			{
				_lastKeys.Enqueue(keyCode);
				_is = true;
			}
			if (_lastKeys.Count > 5)
			{
				_lastKeys.Dequeue();
			}
		}
		if (_is)
		{
			switch (GetWord())
			{
			case "VITEK":
				BinData1();
				break;
			case "TWIND":
				BinData2();
				break;
			case "CHIRV":
				BinData3();
				break;
			case "TOOTH":
				BinData4();
				break;
			case "MMNIM":
				BinData5();
				break;
			case "FMAXF":
				BinData6();
				break;
			}
		}
	}

	private string GetWord()
	{
		string text = "";
		foreach (KeyCode lastKey in _lastKeys)
		{
			text += lastKey;
		}
		return text;
	}

	private void BinData1()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin1");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(-1.5f, 1.4f, -9.255f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 185f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void BinData2()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin2");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(2.999f, 1.405f, -9.255f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -193f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void BinData3()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin3");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(3.241f, 1.4082f, -9.4656f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 90f, 187f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void BinData4()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin4");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(-2.372f, 1.382f, -11.156f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -7.369f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void BinData5()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin5");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(1.543f, 1.386f, -11.156f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -8.09f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void BinData6()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BinaryData/Bin6");
		Texture2D texture2D = new Texture2D(512, 512);
		texture2D.LoadImage(textAsset.bytes);
		Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		material.mainTexture = texture2D;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = new Vector3(-0.456f, 1.548f, -11.156f);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.01f);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 2.947f));
		gameObject.GetComponent<MeshRenderer>().material = material;
	}

	private void Awake()
	{
		DateTime utcNow = DateTime.UtcNow;
		int day = utcNow.Day;
		if (((day == 25 || day == 31) && utcNow.Month == 12) ? true : false)
		{
			Resources.Load<TextAsset>("BinaryData/14523255118");
		}
		if (utcNow.Day == 7 && utcNow.Month == 8 && utcNow.Year > 2024)
		{
			Resources.Load<TextAsset>("BinaryData/29182084125");
		}
	}
}
