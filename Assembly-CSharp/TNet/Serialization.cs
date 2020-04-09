using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TNet
{
	public static class Serialization
	{
		public static BinaryFormatter formatter = new BinaryFormatter();

		private static Dictionary<string, Type> mNameToType = new Dictionary<string, Type>();

		private static Dictionary<Type, string> mTypeToName = new Dictionary<Type, string>();

		private static Dictionary<Type, List<FieldInfo>> mFieldDict = new Dictionary<Type, List<FieldInfo>>();

		private static List<string> mFieldNames = new List<string>();

		private static List<object> mFieldValues = new List<object>();

		public static Type NameToType(string name)
		{
			Type value = null;
			if (!mNameToType.TryGetValue(name, out value))
			{
				if (name == "Vector2")
				{
					value = typeof(Vector2);
				}
				else if (name == "Vector3")
				{
					value = typeof(Vector3);
				}
				else if (name == "Vector4")
				{
					value = typeof(Vector4);
				}
				else if (name == "Euler" || name == "Quaternion")
				{
					value = typeof(Quaternion);
				}
				else if (name == "Rect")
				{
					value = typeof(Rect);
				}
				else if (name == "Color")
				{
					value = typeof(Color);
				}
				else if (name == "Color32")
				{
					value = typeof(Color32);
				}
				else if (name.StartsWith("IList"))
				{
					if (name.Length > 7 && name[5] == '<' && name[name.Length - 1] == '>')
					{
						Type type = NameToType(name.Substring(6, name.Length - 7));
						value = typeof(System.Collections.Generic.List<>).MakeGenericType(type);
					}
					else
					{
						Debug.LogWarning("Malformed type: " + name);
					}
				}
				else if (name.StartsWith("TList"))
				{
					if (name.Length > 7 && name[5] == '<' && name[name.Length - 1] == '>')
					{
						Type type2 = NameToType(name.Substring(6, name.Length - 7));
						value = typeof(List<>).MakeGenericType(type2);
					}
					else
					{
						Debug.LogWarning("Malformed type: " + name);
					}
				}
				else
				{
					value = Type.GetType(name);
				}
				mNameToType[name] = value;
			}
			return value;
		}

		public static string TypeToName(Type type)
		{
			if ((object)type == null)
			{
				Debug.LogError("Type cannot be null");
				return null;
			}
			if (!mTypeToName.TryGetValue(type, out string value))
			{
				if ((object)type == typeof(Vector2))
				{
					value = "Vector2";
				}
				else if ((object)type == typeof(Vector3))
				{
					value = "Vector3";
				}
				else if ((object)type == typeof(Vector4))
				{
					value = "Vector4";
				}
				else if ((object)type == typeof(Quaternion))
				{
					value = "Euler";
				}
				else if ((object)type == typeof(Rect))
				{
					value = "Rect";
				}
				else if ((object)type == typeof(Color))
				{
					value = "Color";
				}
				else if ((object)type == typeof(Color32))
				{
					value = "Color32";
				}
				else if (type.Implements(typeof(IList)))
				{
					Type genericArgument = type.GetGenericArgument();
					value = (((object)genericArgument == null) ? type.ToString() : ("IList<" + genericArgument.ToString() + ">"));
				}
				else if (type.Implements(typeof(TList)))
				{
					Type genericArgument2 = type.GetGenericArgument();
					value = (((object)genericArgument2 == null) ? type.ToString() : ("TList<" + genericArgument2.ToString() + ">"));
				}
				else
				{
					value = type.ToString();
				}
				mTypeToName[type] = value;
			}
			return value;
		}

		public static object ConvertValue(object value, Type desiredType)
		{
			if (value == null)
			{
				return null;
			}
			Type type = value.GetType();
			if (desiredType.IsAssignableFrom(type))
			{
				return value;
			}
			if ((object)type == typeof(int))
			{
				if ((object)desiredType == typeof(byte))
				{
					return (byte)(int)value;
				}
				if ((object)desiredType == typeof(short))
				{
					return (short)(int)value;
				}
				if ((object)desiredType == typeof(ushort))
				{
					return (ushort)(int)value;
				}
			}
			else if ((object)type == typeof(float))
			{
				if ((object)desiredType == typeof(byte))
				{
					return (byte)Mathf.RoundToInt((float)value);
				}
				if ((object)desiredType == typeof(short))
				{
					return (short)Mathf.RoundToInt((float)value);
				}
				if ((object)desiredType == typeof(ushort))
				{
					return (ushort)Mathf.RoundToInt((float)value);
				}
				if ((object)desiredType == typeof(int))
				{
					return Mathf.RoundToInt((float)value);
				}
			}
			else if ((object)type == typeof(Color32))
			{
				if ((object)desiredType == typeof(Color))
				{
					Color32 color = (Color32)value;
					return new Color((float)(int)color.r / 255f, (float)(int)color.g / 255f, (float)(int)color.b / 255f, (float)(int)color.a / 255f);
				}
			}
			else if ((object)type == typeof(Vector3))
			{
				if ((object)desiredType == typeof(Color))
				{
					Vector3 vector = (Vector3)value;
					return new Color(vector.x, vector.y, vector.z);
				}
				if ((object)desiredType == typeof(Quaternion))
				{
					return Quaternion.Euler((Vector3)value);
				}
			}
			else if ((object)type == typeof(Color))
			{
				if ((object)desiredType == typeof(Quaternion))
				{
					Color color2 = (Color)value;
					return new Quaternion(color2.r, color2.g, color2.b, color2.a);
				}
				if ((object)desiredType == typeof(Rect))
				{
					Color color3 = (Color)value;
					return new Rect(color3.r, color3.g, color3.b, color3.a);
				}
				if ((object)desiredType == typeof(Vector4))
				{
					Color color4 = (Color)value;
					return new Vector4(color4.r, color4.g, color4.b, color4.a);
				}
			}
			if (desiredType.IsEnum)
			{
				if ((object)type == typeof(int))
				{
					return value;
				}
				if ((object)type == typeof(string))
				{
					string text = (string)value;
					if (!string.IsNullOrEmpty(text))
					{
						string[] names = Enum.GetNames(desiredType);
						for (int i = 0; i < names.Length; i++)
						{
							if (names[i] == text)
							{
								return Enum.GetValues(desiredType).GetValue(i);
							}
						}
					}
				}
			}
			Debug.LogError("Unable to convert " + value.GetType() + " to " + desiredType);
			return null;
		}

		public static Type GetGenericArgument(this Type type)
		{
			Type[] genericArguments = type.GetGenericArguments();
			return (genericArguments == null || genericArguments.Length != 1) ? null : genericArguments[0];
		}

		public static object Create(this Type type)
		{
			//Discarded unreachable code: IL_000c, IL_0024
			try
			{
				return Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				return null;
			}
		}

		public static object Create(this Type type, int size)
		{
			//Discarded unreachable code: IL_001b, IL_002d
			try
			{
				return Activator.CreateInstance(type, size);
			}
			catch (Exception)
			{
				return type.Create();
			}
		}

		public static List<FieldInfo> GetSerializableFields(this Type type)
		{
			if (!mFieldDict.TryGetValue(type, out List<FieldInfo> value))
			{
				value = new List<FieldInfo>();
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				bool flag = type.IsDefined(typeof(SerializableAttribute), inherit: true);
				int i = 0;
				for (int num = fields.Length; i < num; i++)
				{
					FieldInfo fieldInfo = fields[i];
					if ((fieldInfo.Attributes & FieldAttributes.Static) == 0 && (fieldInfo.IsDefined(typeof(SerializeField), inherit: true) || (flag && (fieldInfo.Attributes & FieldAttributes.Public) != 0)) && !fieldInfo.IsDefined(typeof(NonSerializedAttribute), inherit: true))
					{
						value.Add(fieldInfo);
					}
				}
				mFieldDict[type] = value;
			}
			return value;
		}

		public static FieldInfo GetSerializableField(this Type type, string name)
		{
			List<FieldInfo> serializableFields = type.GetSerializableFields();
			int i = 0;
			for (int size = serializableFields.size; i < size; i++)
			{
				FieldInfo fieldInfo = serializableFields[i];
				if (fieldInfo.Name == name)
				{
					return fieldInfo;
				}
			}
			return null;
		}

		public static bool SetSerializableField(this object obj, string name, object value)
		{
			//Discarded unreachable code: IL_0048
			if (obj == null)
			{
				return false;
			}
			FieldInfo serializableField = obj.GetType().GetSerializableField(name);
			if ((object)serializableField == null)
			{
				return false;
			}
			try
			{
				serializableField.SetValue(obj, ConvertValue(value, serializableField.FieldType));
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}
			return true;
		}

		public static int Deserialize(this object obj, string path)
		{
			FileStream fileStream = File.Open(path, FileMode.Create);
			if (fileStream == null)
			{
				return 0;
			}
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.WriteObject(obj);
			int result = (int)fileStream.Position;
			binaryWriter.Close();
			return result;
		}

		public static void WriteInt(this BinaryWriter bw, int val)
		{
			if (val < 255)
			{
				bw.Write((byte)val);
				return;
			}
			bw.Write(byte.MaxValue);
			bw.Write(val);
		}

		public static void Write(this BinaryWriter writer, Vector2 v)
		{
			writer.Write(v.x);
			writer.Write(v.y);
		}

		public static void Write(this BinaryWriter writer, Vector3 v)
		{
			writer.Write(v.x);
			writer.Write(v.y);
			writer.Write(v.z);
		}

		public static void Write(this BinaryWriter writer, Vector4 v)
		{
			writer.Write(v.x);
			writer.Write(v.y);
			writer.Write(v.z);
			writer.Write(v.w);
		}

		public static void Write(this BinaryWriter writer, Quaternion q)
		{
			writer.Write(q.x);
			writer.Write(q.y);
			writer.Write(q.z);
			writer.Write(q.w);
		}

		public static void Write(this BinaryWriter writer, Color32 c)
		{
			writer.Write(c.r);
			writer.Write(c.g);
			writer.Write(c.b);
			writer.Write(c.a);
		}

		public static void Write(this BinaryWriter writer, Color c)
		{
			writer.Write(c.r);
			writer.Write(c.g);
			writer.Write(c.b);
			writer.Write(c.a);
		}

		public static void Write(this BinaryWriter writer, DataNode node)
		{
			writer.Write(node.name);
			writer.WriteObject(node.value);
			writer.WriteInt(node.children.size);
			int i = 0;
			for (int size = node.children.size; i < size; i++)
			{
				writer.Write(node.children[i]);
			}
		}

		private static int GetPrefix(Type type)
		{
			if ((object)type == typeof(bool))
			{
				return 1;
			}
			if ((object)type == typeof(byte))
			{
				return 2;
			}
			if ((object)type == typeof(ushort))
			{
				return 3;
			}
			if ((object)type == typeof(int))
			{
				return 4;
			}
			if ((object)type == typeof(uint))
			{
				return 5;
			}
			if ((object)type == typeof(float))
			{
				return 6;
			}
			if ((object)type == typeof(string))
			{
				return 7;
			}
			if ((object)type == typeof(Vector2))
			{
				return 8;
			}
			if ((object)type == typeof(Vector3))
			{
				return 9;
			}
			if ((object)type == typeof(Vector4))
			{
				return 10;
			}
			if ((object)type == typeof(Quaternion))
			{
				return 11;
			}
			if ((object)type == typeof(Color32))
			{
				return 12;
			}
			if ((object)type == typeof(Color))
			{
				return 13;
			}
			if ((object)type == typeof(DataNode))
			{
				return 14;
			}
			if ((object)type == typeof(double))
			{
				return 15;
			}
			if ((object)type == typeof(short))
			{
				return 16;
			}
			if ((object)type == typeof(bool[]))
			{
				return 101;
			}
			if ((object)type == typeof(byte[]))
			{
				return 102;
			}
			if ((object)type == typeof(ushort[]))
			{
				return 103;
			}
			if ((object)type == typeof(int[]))
			{
				return 104;
			}
			if ((object)type == typeof(uint[]))
			{
				return 105;
			}
			if ((object)type == typeof(float[]))
			{
				return 106;
			}
			if ((object)type == typeof(string[]))
			{
				return 107;
			}
			if ((object)type == typeof(Vector2[]))
			{
				return 108;
			}
			if ((object)type == typeof(Vector3[]))
			{
				return 109;
			}
			if ((object)type == typeof(Vector4[]))
			{
				return 110;
			}
			if ((object)type == typeof(Quaternion[]))
			{
				return 111;
			}
			if ((object)type == typeof(Color32[]))
			{
				return 112;
			}
			if ((object)type == typeof(Color[]))
			{
				return 113;
			}
			if ((object)type == typeof(double[]))
			{
				return 115;
			}
			if ((object)type == typeof(short[]))
			{
				return 116;
			}
			return 254;
		}

		private static Type GetType(int prefix)
		{
			switch (prefix)
			{
			case 1:
				return typeof(bool);
			case 2:
				return typeof(byte);
			case 3:
				return typeof(ushort);
			case 4:
				return typeof(int);
			case 5:
				return typeof(uint);
			case 6:
				return typeof(float);
			case 7:
				return typeof(string);
			case 8:
				return typeof(Vector2);
			case 9:
				return typeof(Vector3);
			case 10:
				return typeof(Vector4);
			case 11:
				return typeof(Quaternion);
			case 12:
				return typeof(Color32);
			case 13:
				return typeof(Color);
			case 14:
				return typeof(DataNode);
			case 15:
				return typeof(double);
			case 16:
				return typeof(short);
			case 101:
				return typeof(bool[]);
			case 102:
				return typeof(byte[]);
			case 103:
				return typeof(ushort[]);
			case 104:
				return typeof(int[]);
			case 105:
				return typeof(uint[]);
			case 106:
				return typeof(float[]);
			case 107:
				return typeof(string[]);
			case 108:
				return typeof(Vector2[]);
			case 109:
				return typeof(Vector3[]);
			case 110:
				return typeof(Vector4[]);
			case 111:
				return typeof(Quaternion[]);
			case 112:
				return typeof(Color32[]);
			case 113:
				return typeof(Color[]);
			case 115:
				return typeof(double[]);
			case 116:
				return typeof(short[]);
			default:
				return null;
			}
		}

		public static void Write(this BinaryWriter bw, Type type)
		{
			int prefix = GetPrefix(type);
			bw.Write((byte)prefix);
			if (prefix > 250)
			{
				bw.Write(TypeToName(type));
			}
		}

		public static void Write(this BinaryWriter bw, int prefix, Type type)
		{
			bw.Write((byte)prefix);
			if (prefix > 250)
			{
				bw.Write(TypeToName(type));
			}
		}

		public static void WriteObject(this BinaryWriter bw, object obj)
		{
			bw.WriteObject(obj, 255, typeIsKnown: false, useReflection: true);
		}

		public static void WriteObject(this BinaryWriter bw, object obj, bool useReflection)
		{
			bw.WriteObject(obj, 255, typeIsKnown: false, useReflection);
		}

		private static void WriteObject(this BinaryWriter bw, object obj, int prefix, bool typeIsKnown, bool useReflection)
		{
			if (obj == null)
			{
				bw.Write((byte)0);
				return;
			}
			if (obj is IBinarySerializable)
			{
				if (!typeIsKnown)
				{
					bw.Write(253, obj.GetType());
				}
				(obj as IBinarySerializable).Serialize(bw);
				return;
			}
			Type type;
			if (!typeIsKnown)
			{
				type = obj.GetType();
				prefix = GetPrefix(type);
			}
			else
			{
				type = GetType(prefix);
			}
			if (prefix > 250)
			{
				if (obj is TList)
				{
					if (useReflection)
					{
						Type genericArgument = type.GetGenericArgument();
						if ((object)genericArgument != null)
						{
							TList tList = obj as TList;
							int prefix2 = GetPrefix(genericArgument);
							bool flag = true;
							int i = 0;
							for (int count = tList.Count; i < count; i++)
							{
								object obj2 = tList.Get(i);
								if (obj2 != null && (object)genericArgument != obj2.GetType())
								{
									flag = false;
									prefix2 = 255;
									break;
								}
							}
							if (!typeIsKnown)
							{
								bw.Write((byte)98);
							}
							bw.Write(genericArgument);
							bw.Write((byte)(flag ? 1 : 0));
							bw.WriteInt(tList.Count);
							int j = 0;
							for (int count2 = tList.Count; j < count2; j++)
							{
								bw.WriteObject(tList.Get(j), prefix2, flag, useReflection);
							}
							return;
						}
					}
					if (!typeIsKnown)
					{
						bw.Write(byte.MaxValue);
					}
					formatter.Serialize(bw.BaseStream, obj);
					return;
				}
				if (obj is IList)
				{
					if (useReflection)
					{
						Type type2 = type.GetGenericArgument();
						bool flag2 = false;
						if ((object)type2 == null)
						{
							type2 = type.GetElementType();
							flag2 = ((object)type != null);
						}
						if (flag2 || (object)type2 != null)
						{
							int prefix3 = GetPrefix(type2);
							IList list = obj as IList;
							bool flag3 = true;
							foreach (object item in list)
							{
								if (item != null && (object)type2 != item.GetType())
								{
									flag3 = false;
									prefix3 = 255;
									break;
								}
							}
							if (!typeIsKnown)
							{
								bw.Write((byte)((!flag2) ? 99 : 100));
							}
							bw.Write(type);
							bw.Write((byte)(flag3 ? 1 : 0));
							bw.WriteInt(list.Count);
							foreach (object item2 in list)
							{
								bw.WriteObject(item2, prefix3, flag3, useReflection);
							}
							return;
						}
					}
					if (!typeIsKnown)
					{
						bw.Write(byte.MaxValue);
					}
					formatter.Serialize(bw.BaseStream, obj);
					return;
				}
			}
			if (!typeIsKnown)
			{
				bw.Write(prefix, type);
			}
			switch (prefix)
			{
			case 1:
				bw.Write((bool)obj);
				break;
			case 2:
				bw.Write((byte)obj);
				break;
			case 3:
				bw.Write((ushort)obj);
				break;
			case 4:
				bw.Write((int)obj);
				break;
			case 5:
				bw.Write((uint)obj);
				break;
			case 6:
				bw.Write((float)obj);
				break;
			case 7:
				bw.Write((string)obj);
				break;
			case 8:
				bw.Write((Vector2)obj);
				break;
			case 9:
				bw.Write((Vector3)obj);
				break;
			case 10:
				bw.Write((Vector4)obj);
				break;
			case 11:
				bw.Write((Quaternion)obj);
				break;
			case 12:
				bw.Write((Color32)obj);
				break;
			case 13:
				bw.Write((Color)obj);
				break;
			case 14:
				bw.Write((DataNode)obj);
				break;
			case 15:
				bw.Write((double)obj);
				break;
			case 16:
				bw.Write((short)obj);
				break;
			case 101:
			{
				bool[] array15 = (bool[])obj;
				bw.WriteInt(array15.Length);
				int num24 = 0;
				for (int num25 = array15.Length; num24 < num25; num24++)
				{
					bw.Write(array15[num24]);
				}
				break;
			}
			case 102:
			{
				byte[] array14 = (byte[])obj;
				bw.WriteInt(array14.Length);
				bw.Write(array14);
				break;
			}
			case 103:
			{
				ushort[] array13 = (ushort[])obj;
				bw.WriteInt(array13.Length);
				int num22 = 0;
				for (int num23 = array13.Length; num22 < num23; num22++)
				{
					bw.Write(array13[num22]);
				}
				break;
			}
			case 104:
			{
				int[] array12 = (int[])obj;
				bw.WriteInt(array12.Length);
				int num20 = 0;
				for (int num21 = array12.Length; num20 < num21; num20++)
				{
					bw.Write(array12[num20]);
				}
				break;
			}
			case 105:
			{
				uint[] array11 = (uint[])obj;
				bw.WriteInt(array11.Length);
				int num18 = 0;
				for (int num19 = array11.Length; num18 < num19; num18++)
				{
					bw.Write(array11[num18]);
				}
				break;
			}
			case 106:
			{
				float[] array10 = (float[])obj;
				bw.WriteInt(array10.Length);
				int num16 = 0;
				for (int num17 = array10.Length; num16 < num17; num16++)
				{
					bw.Write(array10[num16]);
				}
				break;
			}
			case 107:
			{
				string[] array9 = (string[])obj;
				bw.WriteInt(array9.Length);
				int num14 = 0;
				for (int num15 = array9.Length; num14 < num15; num14++)
				{
					bw.Write(array9[num14]);
				}
				break;
			}
			case 108:
			{
				Vector2[] array8 = (Vector2[])obj;
				bw.WriteInt(array8.Length);
				int num12 = 0;
				for (int num13 = array8.Length; num12 < num13; num12++)
				{
					bw.Write(array8[num12]);
				}
				break;
			}
			case 109:
			{
				Vector3[] array7 = (Vector3[])obj;
				bw.WriteInt(array7.Length);
				int num10 = 0;
				for (int num11 = array7.Length; num10 < num11; num10++)
				{
					bw.Write(array7[num10]);
				}
				break;
			}
			case 110:
			{
				Vector4[] array6 = (Vector4[])obj;
				bw.WriteInt(array6.Length);
				int num8 = 0;
				for (int num9 = array6.Length; num8 < num9; num8++)
				{
					bw.Write(array6[num8]);
				}
				break;
			}
			case 111:
			{
				Quaternion[] array5 = (Quaternion[])obj;
				bw.WriteInt(array5.Length);
				int num6 = 0;
				for (int num7 = array5.Length; num6 < num7; num6++)
				{
					bw.Write(array5[num6]);
				}
				break;
			}
			case 112:
			{
				Color32[] array4 = (Color32[])obj;
				bw.WriteInt(array4.Length);
				int num4 = 0;
				for (int num5 = array4.Length; num4 < num5; num4++)
				{
					bw.Write(array4[num4]);
				}
				break;
			}
			case 113:
			{
				Color[] array3 = (Color[])obj;
				bw.WriteInt(array3.Length);
				int n = 0;
				for (int num3 = array3.Length; n < num3; n++)
				{
					bw.Write(array3[n]);
				}
				break;
			}
			case 115:
			{
				double[] array2 = (double[])obj;
				bw.WriteInt(array2.Length);
				int m = 0;
				for (int num2 = array2.Length; m < num2; m++)
				{
					bw.Write(array2[m]);
				}
				break;
			}
			case 116:
			{
				short[] array = (short[])obj;
				bw.WriteInt(array.Length);
				int l = 0;
				for (int num = array.Length; l < num; l++)
				{
					bw.Write(array[l]);
				}
				break;
			}
			case 254:
			{
				FilterFields(obj);
				bw.WriteInt(mFieldNames.size);
				int k = 0;
				for (int size = mFieldNames.size; k < size; k++)
				{
					bw.Write(mFieldNames[k]);
					bw.WriteObject(mFieldValues[k]);
				}
				break;
			}
			case 255:
				formatter.Serialize(bw.BaseStream, obj);
				break;
			default:
				Debug.LogError("Prefix " + prefix + " is not supported");
				break;
			}
		}

		private static void FilterFields(object obj)
		{
			Type type = obj.GetType();
			List<FieldInfo> serializableFields = type.GetSerializableFields();
			mFieldNames.Clear();
			mFieldValues.Clear();
			for (int i = 0; i < serializableFields.size; i++)
			{
				FieldInfo fieldInfo = serializableFields[i];
				object value = fieldInfo.GetValue(obj);
				if (value != null)
				{
					mFieldNames.Add(fieldInfo.Name);
					mFieldValues.Add(value);
				}
			}
		}

		public static bool Implements(this Type t, Type interfaceType)
		{
			return interfaceType?.IsAssignableFrom(t) ?? false;
		}

		public static int ReadInt(this BinaryReader reader)
		{
			int num = reader.ReadByte();
			if (num == 255)
			{
				num = reader.ReadInt32();
			}
			return num;
		}

		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			return new Vector2(reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector4 ReadVector4(this BinaryReader reader)
		{
			return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Quaternion ReadQuaternion(this BinaryReader reader)
		{
			return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Color32 ReadColor32(this BinaryReader reader)
		{
			return new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		}

		public static Color ReadColor(this BinaryReader reader)
		{
			return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static DataNode ReadDataNode(this BinaryReader reader)
		{
			DataNode dataNode = new DataNode();
			dataNode.name = reader.ReadString();
			dataNode.value = reader.ReadObject();
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				dataNode.children.Add(reader.ReadDataNode());
			}
			return dataNode;
		}

		public static Type ReadType(this BinaryReader reader)
		{
			int num = reader.ReadByte();
			return (num <= 250) ? GetType(num) : NameToType(reader.ReadString());
		}

		public static Type ReadType(this BinaryReader reader, out int prefix)
		{
			prefix = reader.ReadByte();
			return (prefix <= 250) ? GetType(prefix) : NameToType(reader.ReadString());
		}

		public static T ReadObject<T>(this BinaryReader reader)
		{
			object obj = reader.ReadObject();
			if (obj == null)
			{
				return default(T);
			}
			return (T)obj;
		}

		public static object ReadObject(this BinaryReader reader)
		{
			return reader.ReadObject(null, 0, null, typeIsKnown: false);
		}

		public static object ReadObject(this BinaryReader reader, object obj)
		{
			return reader.ReadObject(obj, 0, null, typeIsKnown: false);
		}

		private static object ReadObject(this BinaryReader reader, object obj, int prefix, Type type, bool typeIsKnown)
		{
			if (!typeIsKnown)
			{
				type = reader.ReadType(out prefix);
			}
			if (type.Implements(typeof(IBinarySerializable)))
			{
				prefix = 253;
			}
			switch (prefix)
			{
			case 0:
				return null;
			case 1:
				return reader.ReadBoolean();
			case 2:
				return reader.ReadByte();
			case 3:
				return reader.ReadUInt16();
			case 4:
				return reader.ReadInt32();
			case 5:
				return reader.ReadUInt32();
			case 6:
				return reader.ReadSingle();
			case 7:
				return reader.ReadString();
			case 8:
				return reader.ReadVector2();
			case 9:
				return reader.ReadVector3();
			case 10:
				return reader.ReadVector4();
			case 11:
				return reader.ReadQuaternion();
			case 12:
				return reader.ReadColor32();
			case 13:
				return reader.ReadColor();
			case 14:
				return reader.ReadDataNode();
			case 15:
				return reader.ReadDouble();
			case 16:
				return reader.ReadInt16();
			case 98:
			{
				type = reader.ReadType(out prefix);
				bool typeIsKnown2 = reader.ReadByte() == 1;
				int num3 = reader.ReadInt();
				TList tList = null;
				if (obj != null)
				{
					tList = (TList)obj;
				}
				else
				{
					Type type2 = typeof(List<>).MakeGenericType(type);
					tList = (TList)Activator.CreateInstance(type2);
				}
				for (int k = 0; k < num3; k++)
				{
					object obj2 = reader.ReadObject(null, prefix, type, typeIsKnown2);
					tList?.Add(obj2);
				}
				return tList;
			}
			case 99:
			{
				type = reader.ReadType(out prefix);
				bool typeIsKnown4 = reader.ReadByte() == 1;
				int num27 = reader.ReadInt();
				IList list2 = null;
				if (obj != null)
				{
					list2 = (IList)obj;
				}
				else
				{
					Type type3 = typeof(System.Collections.Generic.List<>).MakeGenericType(type);
					list2 = (IList)Activator.CreateInstance(type3);
				}
				for (int num28 = 0; num28 < num27; num28++)
				{
					object value2 = reader.ReadObject(null, prefix, type, typeIsKnown4);
					list2?.Add(value2);
				}
				return list2;
			}
			case 100:
			{
				type = reader.ReadType(out prefix);
				bool typeIsKnown3 = reader.ReadByte() == 1;
				int num21 = reader.ReadInt();
				IList list = (IList)type.Create(num21);
				if (list != null)
				{
					type = type.GetElementType();
					prefix = GetPrefix(type);
					for (int num22 = 0; num22 < num21; num22++)
					{
						list[num22] = reader.ReadObject(null, prefix, type, typeIsKnown3);
					}
				}
				else
				{
					Debug.LogError("Failed to create a " + type);
				}
				return list;
			}
			case 101:
			{
				int num15 = reader.ReadInt();
				bool[] array9 = new bool[num15];
				for (int num16 = 0; num16 < num15; num16++)
				{
					array9[num16] = reader.ReadBoolean();
				}
				return array9;
			}
			case 102:
			{
				int count = reader.ReadInt();
				return reader.ReadBytes(count);
			}
			case 103:
			{
				int num9 = reader.ReadInt();
				ushort[] array6 = new ushort[num9];
				for (int num10 = 0; num10 < num9; num10++)
				{
					array6[num10] = reader.ReadUInt16();
				}
				return array6;
			}
			case 104:
			{
				int num6 = reader.ReadInt();
				int[] array4 = new int[num6];
				for (int n = 0; n < num6; n++)
				{
					array4[n] = reader.ReadInt32();
				}
				return array4;
			}
			case 105:
			{
				int num5 = reader.ReadInt();
				uint[] array3 = new uint[num5];
				for (int m = 0; m < num5; m++)
				{
					array3[m] = reader.ReadUInt32();
				}
				return array3;
			}
			case 106:
			{
				int num4 = reader.ReadInt();
				float[] array2 = new float[num4];
				for (int l = 0; l < num4; l++)
				{
					array2[l] = reader.ReadSingle();
				}
				return array2;
			}
			case 107:
			{
				int num2 = reader.ReadInt();
				string[] array = new string[num2];
				for (int j = 0; j < num2; j++)
				{
					array[j] = reader.ReadString();
				}
				return array;
			}
			case 108:
			{
				int num29 = reader.ReadInt();
				Vector2[] array14 = new Vector2[num29];
				for (int num30 = 0; num30 < num29; num30++)
				{
					array14[num30] = reader.ReadVector2();
				}
				return array14;
			}
			case 109:
			{
				int num25 = reader.ReadInt();
				Vector3[] array13 = new Vector3[num25];
				for (int num26 = 0; num26 < num25; num26++)
				{
					array13[num26] = reader.ReadVector3();
				}
				return array13;
			}
			case 110:
			{
				int num23 = reader.ReadInt();
				Vector4[] array12 = new Vector4[num23];
				for (int num24 = 0; num24 < num23; num24++)
				{
					array12[num24] = reader.ReadVector4();
				}
				return array12;
			}
			case 111:
			{
				int num19 = reader.ReadInt();
				Quaternion[] array11 = new Quaternion[num19];
				for (int num20 = 0; num20 < num19; num20++)
				{
					array11[num20] = reader.ReadQuaternion();
				}
				return array11;
			}
			case 112:
			{
				int num17 = reader.ReadInt();
				Color32[] array10 = new Color32[num17];
				for (int num18 = 0; num18 < num17; num18++)
				{
					array10[num18] = reader.ReadColor32();
				}
				return array10;
			}
			case 113:
			{
				int num13 = reader.ReadInt();
				Color[] array8 = new Color[num13];
				for (int num14 = 0; num14 < num13; num14++)
				{
					array8[num14] = reader.ReadColor();
				}
				return array8;
			}
			case 115:
			{
				int num11 = reader.ReadInt();
				double[] array7 = new double[num11];
				for (int num12 = 0; num12 < num11; num12++)
				{
					array7[num12] = reader.ReadDouble();
				}
				return array7;
			}
			case 116:
			{
				int num7 = reader.ReadInt();
				short[] array5 = new short[num7];
				for (int num8 = 0; num8 < num7; num8++)
				{
					array5[num8] = reader.ReadInt16();
				}
				return array5;
			}
			case 253:
			{
				object obj3;
				if (obj != null)
				{
					IBinarySerializable binarySerializable = (IBinarySerializable)obj;
					obj3 = binarySerializable;
				}
				else
				{
					obj3 = (IBinarySerializable)type.Create();
				}
				IBinarySerializable binarySerializable2 = (IBinarySerializable)obj3;
				binarySerializable2?.Deserialize(reader);
				return binarySerializable2;
			}
			case 254:
				if (obj == null)
				{
					obj = type.Create();
					if (obj == null)
					{
						Debug.LogError("Unable to create an instance of " + type);
					}
				}
				if (obj != null)
				{
					int num = reader.ReadInt();
					for (int i = 0; i < num; i++)
					{
						string text = reader.ReadString();
						if (string.IsNullOrEmpty(text))
						{
							Debug.LogError("Null field specified when serializing " + type);
							continue;
						}
						FieldInfo field = type.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						object value = reader.ReadObject();
						if ((object)field != null)
						{
							field.SetValue(obj, ConvertValue(value, field.FieldType));
						}
						else
						{
							Debug.LogError("Unable to set field " + type + "." + text);
						}
					}
				}
				return obj;
			case 255:
				return formatter.Deserialize(reader.BaseStream);
			default:
				Debug.LogError("Unknown prefix: " + prefix);
				return null;
			}
		}
	}
}
