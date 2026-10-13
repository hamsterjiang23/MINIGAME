using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AesCrypter
{
	private const string Key = "e38c9861b43264e37c1eef64d7925dde";

	private static byte[] GenerateIV()
	{
		byte[] array = new byte[16];
		using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		randomNumberGenerator.GetBytes(array);
		return array;
	}

	public static string Encrypt(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("e38c9861b43264e37c1eef64d7925dde");
		byte[] array = GenerateIV();
		using Aes aes = Aes.Create();
		aes.Key = bytes;
		aes.IV = array;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(array, 0, array.Length);
		using (CryptoStream stream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(text);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static string Decrypt(string encryptedText)
	{
		if (string.IsNullOrEmpty(encryptedText))
		{
			Debug.LogError("Encrypted text is empty");
			return "";
		}
		byte[] bytes = Encoding.UTF8.GetBytes("e38c9861b43264e37c1eef64d7925dde");
		byte[] array = Convert.FromBase64String(encryptedText);
		using Aes aes = Aes.Create();
		aes.Key = bytes;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		byte[] array2 = new byte[16];
		Array.Copy(array, 0, array2, 0, array2.Length);
		aes.IV = array2;
		using MemoryStream stream = new MemoryStream(array, 16, array.Length - 16);
		using CryptoStream stream2 = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}
}
