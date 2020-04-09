using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Flowmap
{
	public class TextureUtilities
	{
		public struct FileTextureFormat
		{
			public string name;

			public string extension;

			public FileTextureFormat(string name, string extension)
			{
				this.name = name;
				this.extension = extension;
			}
		}

		private class ColorArrayThreadedInfo
		{
			public int start;

			public int length;

			public ManualResetEvent resetEvent;

			public Color[] colorArray;

			public float[,] heightArray;

			public int resX;

			public int resY;

			public ColorArrayThreadedInfo(int start, int length, ref Color[] colors, int resX, int resY, float[,] heights, ManualResetEvent resetEvent)
			{
				this.start = start;
				this.length = length;
				this.resetEvent = resetEvent;
				colorArray = colors;
				this.resX = resX;
				this.resY = resY;
				heightArray = heights;
			}
		}

		public static FileTextureFormat[] SupportedFormats = new FileTextureFormat[2]
		{
			new FileTextureFormat("Tga", "tga"),
			new FileTextureFormat("Png", "png")
		};

		public static FileTextureFormat[] SupportedRawFormats = new FileTextureFormat[1]
		{
			new FileTextureFormat("Raw", "raw")
		};

		public static string[] GetSupportedFormatsWithExtension()
		{
			string[] array = new string[SupportedFormats.Length];
			for (int i = 0; i < SupportedFormats.Length; i++)
			{
				array[i] = SupportedFormats[i].name + " (*." + SupportedFormats[i].extension + ")";
			}
			return array;
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, FileTextureFormat format)
		{
			WriteRenderTextureToFile(textureToWrite, filename, linear: false, format);
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, bool linear, FileTextureFormat format)
		{
			Texture2D texture2D = new Texture2D(textureToWrite.width, textureToWrite.height, TextureFormat.ARGB32, mipmap: false, linear);
			RenderTexture temporary = RenderTexture.GetTemporary(textureToWrite.width, textureToWrite.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Graphics.Blit(textureToWrite, temporary);
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			texture2D.Apply(updateMipmaps: false);
			WriteTexture2DToFile(texture2D, filename, format);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, bool linear, FileTextureFormat format, string customShader)
		{
			Texture2D texture2D = new Texture2D(textureToWrite.width, textureToWrite.height, TextureFormat.ARGB32, mipmap: false, linear);
			RenderTexture temporary = RenderTexture.GetTemporary(textureToWrite.width, textureToWrite.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Material material = new Material(Shader.Find(customShader));
			material.SetTexture("_RenderTex", textureToWrite);
			Graphics.Blit(null, temporary, material);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(material);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(material);
			}
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			texture2D.Apply(updateMipmaps: false);
			WriteTexture2DToFile(texture2D, filename, format);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		public static void WriteTexture2DToFile(Texture2D textureToWrite, string filename, FileTextureFormat format)
		{
			byte[] bytes = null;
			switch (format.name)
			{
			case "Png":
				bytes = textureToWrite.EncodeToPNG();
				break;
			case "Tga":
				bytes = EncodeToTGA(textureToWrite);
				break;
			}
			if (!filename.EndsWith("." + format.extension))
			{
				filename = filename + "." + format.extension;
			}
			File.WriteAllBytes(filename, bytes);
		}

		public static Color SampleColorBilinear(Color[] data, int resolutionX, int resolutionY, float u, float v)
		{
			u = Mathf.Clamp(u * (float)(resolutionX - 1), 0f, resolutionX - 1);
			v = Mathf.Clamp(v * (float)(resolutionY - 1), 0f, resolutionY - 1);
			if (Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v) >= data.Length || Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v) < 0)
			{
				Debug.Log("out of range " + u + " " + v + " " + resolutionX + " " + resolutionY);
				return Color.black;
			}
			Color a = data[Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v)];
			Color a2 = data[Mathf.CeilToInt(u) + resolutionX * Mathf.FloorToInt(v)];
			Color a3 = data[Mathf.FloorToInt(u) + resolutionX * Mathf.CeilToInt(v)];
			Color a4 = data[Mathf.CeilToInt(u) + resolutionX * Mathf.CeilToInt(v)];
			float num = Mathf.Floor(u);
			float num2 = Mathf.Floor(u + 1f);
			float num3 = Mathf.Floor(v);
			float num4 = Mathf.Floor(v + 1f);
			Color a5 = (num2 - u) / (num2 - num) * a + (u - num) / (num2 - num) * a2;
			Color a6 = (num2 - u) / (num2 - num) * a3 + (u - num) / (num2 - num) * a4;
			return (num4 - v) / (num4 - num3) * a5 + (v - num3) / (num4 - num3) * a6;
		}

		public static float[,] ReadRawImage(string path, int resX, int resY, bool pcByteOrder)
		{
			float[,] array = new float[resX, resY];
			FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(input);
			for (int num = resY - 1; num > -1; num--)
			{
				for (int i = 0; i < resX; i++)
				{
					byte[] array2 = binaryReader.ReadBytes(2);
					if (!pcByteOrder)
					{
						byte b = array2[0];
						array2[0] = array2[1];
						array2[1] = b;
					}
					ushort num2 = BitConverter.ToUInt16(array2, 0);
					array[i, num] = (float)(int)num2 / 65536f;
				}
			}
			binaryReader.Close();
			return array;
		}

		public static Texture2D ReadRawImageToTexture(string path, int resX, int resY, bool pcByteOrder)
		{
			float[,] heights = ReadRawImage(path, resX, resY, pcByteOrder);
			Texture2D texture2D = new Texture2D(resX, resY, TextureFormat.ARGB32, mipmap: false, linear: true);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.anisoLevel = 9;
			texture2D.filterMode = FilterMode.Trilinear;
			int processorCount = SystemInfo.processorCount;
			int num = Mathf.CeilToInt(resY / processorCount);
			Color[] colors = new Color[resX * resY];
			ManualResetEvent[] array = new ManualResetEvent[processorCount];
			for (int i = 0; i < processorCount; i++)
			{
				int start = i * num;
				int length = (i != processorCount - 1) ? num : (resX - 1 - i * num);
				array[i] = new ManualResetEvent(initialState: false);
				ThreadPool.QueueUserWorkItem(ThreadedEncodeFloat, new ColorArrayThreadedInfo(start, length, ref colors, resX, resY, heights, array[i]));
			}
			WaitHandle.WaitAll(array);
			texture2D.SetPixels(colors);
			texture2D.Apply(updateMipmaps: false);
			return texture2D;
		}

		private static void ThreadedEncodeFloat(object info)
		{
			ColorArrayThreadedInfo colorArrayThreadedInfo = info as ColorArrayThreadedInfo;
			try
			{
				for (int i = colorArrayThreadedInfo.start; i < colorArrayThreadedInfo.start + colorArrayThreadedInfo.length; i++)
				{
					for (int j = 0; j < colorArrayThreadedInfo.resY; j++)
					{
						colorArrayThreadedInfo.colorArray[i + j * colorArrayThreadedInfo.resX] = EncodeFloatRGBA(colorArrayThreadedInfo.heightArray[i, j]);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
			}
			colorArrayThreadedInfo.resetEvent.Set();
		}

		public static Texture2D GetRawPreviewTexture(Texture2D rawTexture)
		{
			Texture2D texture2D = new Texture2D(rawTexture.width, rawTexture.height, TextureFormat.ARGB32, mipmap: true, linear: true);
			Color[] array = new Color[texture2D.width * texture2D.height];
			for (int i = 0; i < texture2D.height; i++)
			{
				for (int j = 0; j < texture2D.width; j++)
				{
					float num = DecodeFloatRGBA(rawTexture.GetPixel(j, i));
					array[j + i * texture2D.width] = new Color(num, num, num, 1f);
				}
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		public static Color EncodeFloatRGBA(float v)
		{
			v = Mathf.Min(v, 0.999f);
			Color a = new Color(1f, 255f, 65025f, 160581376f);
			float num = 0.003921569f;
			Color result = a * v;
			result.r -= Mathf.Floor(result.r);
			result.g -= Mathf.Floor(result.g);
			result.b -= Mathf.Floor(result.b);
			result.a -= Mathf.Floor(result.a);
			result.r -= result.g * num;
			result.g -= result.b * num;
			result.b -= result.a * num;
			result.a -= result.a * num;
			return result;
		}

		public static float DecodeFloatRGBA(Color enc)
		{
			return Vector4.Dot(b: new Color(1f, 0.003921569f, 1.53787E-05f, 6.2273724E-09f), a: enc);
		}

		public static Texture2D ImportTGA(string path)
		{
			//Discarded unreachable code: IL_0153, IL_0161
			try
			{
				FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(input);
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadInt16();
				binaryReader.ReadInt16();
				binaryReader.ReadByte();
				binaryReader.ReadInt16();
				binaryReader.ReadInt16();
				short num = binaryReader.ReadInt16();
				short num2 = binaryReader.ReadInt16();
				byte b = binaryReader.ReadByte();
				binaryReader.ReadByte();
				Texture2D texture2D = new Texture2D(num, num2, (b != 32) ? TextureFormat.RGB24 : TextureFormat.ARGB32, mipmap: true);
				Color32[] array = new Color32[num * num2];
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num; j++)
					{
						if (b == 32)
						{
							byte b2 = binaryReader.ReadByte();
							byte g = binaryReader.ReadByte();
							byte r = binaryReader.ReadByte();
							byte a = binaryReader.ReadByte();
							array[j + i * num] = new Color32(r, g, b2, a);
						}
						else
						{
							byte b3 = binaryReader.ReadByte();
							byte g2 = binaryReader.ReadByte();
							byte r2 = binaryReader.ReadByte();
							array[j + i * num] = new Color32(r2, g2, b3, 1);
						}
					}
				}
				texture2D.SetPixels32(array);
				texture2D.Apply();
				return texture2D;
			}
			catch
			{
				return null;
			}
		}

		public static byte[] EncodeToTGA(Texture2D texture)
		{
			List<byte> list = new List<byte>();
			list.Add(0);
			list.Add(0);
			list.Add(2);
			list.AddRange(BitConverter.GetBytes((short)0));
			list.AddRange(BitConverter.GetBytes((short)0));
			list.Add(0);
			list.AddRange(BitConverter.GetBytes((short)0));
			list.AddRange(BitConverter.GetBytes((short)0));
			list.AddRange(BitConverter.GetBytes((short)texture.width));
			list.AddRange(BitConverter.GetBytes((short)texture.height));
			short num = 0;
			switch (texture.format)
			{
			case TextureFormat.ARGB32:
				num = 32;
				break;
			case TextureFormat.RGB24:
				num = 24;
				break;
			}
			list.AddRange(BitConverter.GetBytes(num));
			switch (num)
			{
			case 24:
				list.Add(0);
				break;
			case 32:
				list.Add(8);
				break;
			}
			Color32[] pixels = texture.GetPixels32();
			for (int i = 0; i < texture.height; i++)
			{
				for (int j = 0; j < texture.width; j++)
				{
					list.Add(pixels[j + i * texture.width].g);
					list.Add(pixels[j + i * texture.width].r);
					if (num == 32)
					{
						list.Add(pixels[j + i * texture.width].a);
					}
					list.Add(pixels[j + i * texture.width].b);
				}
			}
			return list.ToArray();
		}
	}
}
