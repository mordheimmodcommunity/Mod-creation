using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TNet
{
	[Serializable]
	public class DataNode
	{
		private object mValue;

		[NonSerialized]
		private bool mResolved = true;

		public string name;

		public List<DataNode> children = new List<DataNode>();

		public object value
		{
			get
			{
				if (!mResolved && !ResolveValue(null))
				{
					children.Clear();
				}
				return mValue;
			}
			set
			{
				mValue = value;
				mResolved = true;
			}
		}

		public bool isSerializable => value != null || children.size > 0;

		public Type type => (value == null) ? typeof(void) : mValue.GetType();

		public DataNode()
		{
		}

		public DataNode(string name)
		{
			this.name = name;
		}

		public DataNode(string name, object value)
		{
			this.name = name;
			this.value = value;
		}

		public void Clear()
		{
			value = null;
			children.Clear();
		}

		public object Get(Type type)
		{
			return Serialization.ConvertValue(value, type);
		}

		public T Get<T>()
		{
			if (value is T)
			{
				return (T)mValue;
			}
			object obj = Get(typeof(T));
			return (mValue == null) ? default(T) : ((T)obj);
		}

		public DataNode AddChild()
		{
			DataNode dataNode = new DataNode();
			children.Add(dataNode);
			return dataNode;
		}

		public DataNode AddChild(string name)
		{
			DataNode dataNode = AddChild();
			dataNode.name = name;
			return dataNode;
		}

		public DataNode AddChild(string name, object value)
		{
			DataNode dataNode = AddChild();
			dataNode.name = name;
			dataNode.value = ((!(value is Enum)) ? value : value.ToString());
			return dataNode;
		}

		public DataNode SetChild(string name, object value)
		{
			DataNode dataNode = GetChild(name);
			if (dataNode == null)
			{
				dataNode = AddChild();
			}
			dataNode.name = name;
			dataNode.value = ((!(value is Enum)) ? value : value.ToString());
			return dataNode;
		}

		public DataNode GetChild(string name)
		{
			for (int i = 0; i < children.size; i++)
			{
				if (children[i].name == name)
				{
					return children[i];
				}
			}
			return null;
		}

		public DataNode GetChild(string name, bool createIfMissing)
		{
			for (int i = 0; i < children.size; i++)
			{
				if (children[i].name == name)
				{
					return children[i];
				}
			}
			if (createIfMissing)
			{
				return AddChild(name);
			}
			return null;
		}

		public T GetChild<T>(string name)
		{
			DataNode child = GetChild(name);
			if (child == null)
			{
				return default(T);
			}
			return child.Get<T>();
		}

		public T GetChild<T>(string name, T defaultValue)
		{
			DataNode child = GetChild(name);
			if (child == null)
			{
				return defaultValue;
			}
			return child.Get<T>();
		}

		public void RemoveChild(string name)
		{
			int num = 0;
			while (true)
			{
				if (num < children.size)
				{
					if (children[num].name == name)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			children.RemoveAt(num);
		}

		public DataNode Clone()
		{
			DataNode dataNode = new DataNode(name);
			dataNode.mValue = mValue;
			dataNode.mResolved = mResolved;
			for (int i = 0; i < children.size; i++)
			{
				dataNode.children.Add(children[i].Clone());
			}
			return dataNode;
		}

		public void Write(string path)
		{
			Write(path, binary: false);
		}

		public static DataNode Read(string path)
		{
			return Read(path, binary: false);
		}

		public void Write(string path, bool binary)
		{
			if (binary)
			{
				FileStream output = File.Create(path);
				BinaryWriter binaryWriter = new BinaryWriter(output);
				binaryWriter.WriteObject(this);
				binaryWriter.Close();
			}
			else
			{
				StreamWriter streamWriter = new StreamWriter(path, append: false);
				Write(streamWriter, 0);
				streamWriter.Close();
			}
		}

		public static DataNode Read(string path, bool binary)
		{
			if (binary)
			{
				FileStream input = File.OpenRead(path);
				BinaryReader binaryReader = new BinaryReader(input);
				DataNode result = binaryReader.ReadObject<DataNode>();
				binaryReader.Close();
				return result;
			}
			StreamReader streamReader = new StreamReader(path);
			DataNode result2 = Read(streamReader);
			streamReader.Close();
			return result2;
		}

		public static DataNode Read(byte[] bytes, bool binary)
		{
			if (bytes == null || bytes.Length == 0)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream(bytes);
			if (binary)
			{
				BinaryReader binaryReader = new BinaryReader(memoryStream);
				DataNode result = binaryReader.ReadObject<DataNode>();
				binaryReader.Close();
				return result;
			}
			StreamReader streamReader = new StreamReader(memoryStream);
			DataNode result2 = Read(streamReader);
			streamReader.Close();
			return result2;
		}

		public void Write(StreamWriter writer)
		{
			Write(writer, 0);
		}

		public static DataNode Read(StreamReader reader)
		{
			string nextLine = GetNextLine(reader);
			int offset = CalculateTabs(nextLine);
			DataNode dataNode = new DataNode();
			dataNode.Read(reader, nextLine, ref offset);
			return dataNode;
		}

		public override string ToString()
		{
			if (!isSerializable)
			{
				return string.Empty;
			}
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter writer = new StreamWriter(memoryStream);
			Write(writer, 0);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			StreamReader streamReader = new StreamReader(memoryStream);
			string result = streamReader.ReadToEnd();
			memoryStream.Close();
			return result;
		}

		private void Write(StreamWriter writer, int tab)
		{
			if (isSerializable)
			{
				Write(writer, tab, name, value, writeType: true);
				for (int i = 0; i < children.size; i++)
				{
					children[i].Write(writer, tab + 1);
				}
			}
			if (tab == 0)
			{
				writer.Flush();
			}
		}

		private static void Write(StreamWriter writer, int tab, string name, object value, bool writeType)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}
			WriteTabs(writer, tab);
			writer.Write(Escape(name));
			if (value == null)
			{
				writer.Write('\n');
				return;
			}
			Type type = value.GetType();
			if ((object)type == typeof(string))
			{
				writer.Write(" = \"");
				writer.Write((string)value);
				writer.Write('"');
				writer.Write('\n');
				return;
			}
			if ((object)type == typeof(bool))
			{
				writer.Write(" = ");
				writer.Write((!(bool)value) ? "false" : "true");
				writer.Write('\n');
				return;
			}
			if ((object)type == typeof(int) || (object)type == typeof(float) || (object)type == typeof(uint) || (object)type == typeof(byte) || (object)type == typeof(short) || (object)type == typeof(ushort))
			{
				writer.Write(" = ");
				writer.Write(value.ToString());
				writer.Write('\n');
				return;
			}
			if ((object)type == typeof(Vector2))
			{
				Vector2 vector = (Vector2)value;
				writer.Write(" = (");
				writer.Write(vector.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if ((object)type == typeof(Vector3))
			{
				Vector3 vector2 = (Vector3)value;
				writer.Write(" = (");
				writer.Write(vector2.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector2.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector2.z.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if ((object)type == typeof(Color))
			{
				Color color = (Color)value;
				writer.Write(" = (");
				writer.Write(color.r.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(color.g.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(color.b.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(color.a.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if ((object)type == typeof(Color32))
			{
				Color32 color2 = (Color32)value;
				writer.Write(" = 0x");
				if (color2.a == byte.MaxValue)
				{
					writer.Write(((color2.r << 16) | (color2.g << 8) | color2.b).ToString("X6"));
				}
				else
				{
					writer.Write(((color2.r << 24) | (color2.g << 16) | (color2.b << 8) | color2.a).ToString("X8"));
				}
				writer.Write('\n');
				return;
			}
			if ((object)type == typeof(Vector4))
			{
				Vector4 vector3 = (Vector4)value;
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(vector3.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector3.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector3.z.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(vector3.w.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if ((object)type == typeof(Quaternion))
			{
				Vector3 eulerAngles = ((Quaternion)value).eulerAngles;
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(eulerAngles.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(eulerAngles.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(eulerAngles.z.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if ((object)type == typeof(Rect))
			{
				Rect rect = (Rect)value;
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('(');
				writer.Write(rect.x.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(rect.y.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(rect.width.ToString(CultureInfo.InvariantCulture));
				writer.Write(", ");
				writer.Write(rect.height.ToString(CultureInfo.InvariantCulture));
				writer.Write(")\n");
				return;
			}
			if (value is TList)
			{
				TList tList = value as TList;
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');
				if (tList.Count > 0)
				{
					int i = 0;
					for (int count = tList.Count; i < count; i++)
					{
						Write(writer, tab + 1, "Add", tList.Get(i), writeType: false);
					}
				}
				return;
			}
			if (value is IList)
			{
				IList list = value as IList;
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');
				if (list.Count > 0)
				{
					int j = 0;
					for (int count2 = list.Count; j < count2; j++)
					{
						Write(writer, tab + 1, "Add", list[j], writeType: false);
					}
				}
				return;
			}
			if (value is IDataNodeSerializable)
			{
				IDataNodeSerializable dataNodeSerializable = value as IDataNodeSerializable;
				DataNode dataNode = new DataNode();
				dataNodeSerializable.Serialize(dataNode);
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
				writer.Write('\n');
				for (int k = 0; k < dataNode.children.size; k++)
				{
					DataNode dataNode2 = dataNode.children[k];
					dataNode2.Write(writer, tab + 1);
				}
				return;
			}
			if (value is GameObject)
			{
				UnityEngine.Debug.LogError("It's not possible to save game objects.");
				writer.Write('\n');
				return;
			}
			if (value as Component != null)
			{
				UnityEngine.Debug.LogError("It's not possible to save components.");
				writer.Write('\n');
				return;
			}
			if (writeType)
			{
				writer.Write(" = ");
				writer.Write(Serialization.TypeToName(type));
			}
			writer.Write('\n');
			List<FieldInfo> serializableFields = type.GetSerializableFields();
			if (serializableFields.size <= 0)
			{
				return;
			}
			for (int l = 0; l < serializableFields.size; l++)
			{
				FieldInfo fieldInfo = serializableFields[l];
				object value2 = fieldInfo.GetValue(value);
				if (value2 != null)
				{
					Write(writer, tab + 1, fieldInfo.Name, value2, writeType: true);
				}
			}
		}

		private static void WriteTabs(StreamWriter writer, int count)
		{
			for (int i = 0; i < count; i++)
			{
				writer.Write('\t');
			}
		}

		private string Read(StreamReader reader, string line, ref int offset)
		{
			if (line != null)
			{
				int num = offset;
				int num2 = line.IndexOf("=", num);
				if (num2 == -1)
				{
					name = Unescape(line.Substring(offset)).Trim();
					value = null;
				}
				else
				{
					name = Unescape(line.Substring(offset, num2 - offset)).Trim();
					mValue = line.Substring(num2 + 1).Trim();
					mResolved = false;
				}
				line = GetNextLine(reader);
				offset = CalculateTabs(line);
				while (line != null && offset == num + 1)
				{
					line = AddChild().Read(reader, line, ref offset);
				}
			}
			return line;
		}

		private bool ResolveValue(Type type)
		{
			mResolved = true;
			string text = mValue as string;
			if (string.IsNullOrEmpty(text))
			{
				return SetValue(text, type, null);
			}
			if (text.Length > 2)
			{
				if (text[0] == '"' && text[text.Length - 1] == '"')
				{
					mValue = text.Substring(1, text.Length - 2);
					return true;
				}
				if (text[0] == '0' && text[1] == 'x' && text.Length > 7)
				{
					mValue = ParseColor32(text, 2);
					return true;
				}
				if (text[0] == '(' && text[text.Length - 1] == ')')
				{
					text = text.Substring(1, text.Length - 2);
					string[] array = text.Split(new char[1]
					{
						','
					});
					if (array.Length == 1)
					{
						return SetValue(text, typeof(float), null);
					}
					if (array.Length == 2)
					{
						return SetValue(text, typeof(Vector2), array);
					}
					if (array.Length == 3)
					{
						return SetValue(text, typeof(Vector3), array);
					}
					if (array.Length == 4)
					{
						return SetValue(text, typeof(Color), array);
					}
					mValue = text;
					return true;
				}
				if (bool.TryParse(text, out bool result))
				{
					mValue = result;
					return true;
				}
			}
			int num = text.IndexOf('(');
			if (num == -1)
			{
				if (text[0] == '-' || (text[0] >= '0' && text[0] <= '9'))
				{
					int result3;
					if (text.IndexOf('.') != -1)
					{
						if (float.TryParse(text, out float result2))
						{
							mValue = result2;
							return true;
						}
					}
					else if (int.TryParse(text, out result3))
					{
						mValue = result3;
						return true;
					}
				}
			}
			else
			{
				int num2 = (text[text.Length - 1] != ')') ? text.LastIndexOf(')', num) : (text.Length - 1);
				if (num2 != -1 && text.Length > 2)
				{
					string text2 = text.Substring(0, num);
					type = Serialization.NameToType(text2);
					text = text.Substring(num + 1, num2 - num - 1);
				}
				else if ((object)type == null)
				{
					type = typeof(string);
					mValue = text;
					return true;
				}
			}
			if ((object)type == null)
			{
				type = Serialization.NameToType(text);
			}
			return SetValue(text, type, null);
		}

		private bool SetValue(string text, Type type, string[] parts)
		{
			if ((object)type == null || (object)type == typeof(void))
			{
				mValue = null;
			}
			else if ((object)type == typeof(string))
			{
				mValue = text;
			}
			else if ((object)type == typeof(bool))
			{
				bool result = false;
				if (bool.TryParse(text, out result))
				{
					mValue = result;
				}
			}
			else if ((object)type == typeof(byte))
			{
				if (byte.TryParse(text, out byte result2))
				{
					mValue = result2;
				}
			}
			else if ((object)type == typeof(short))
			{
				if (short.TryParse(text, out short result3))
				{
					mValue = result3;
				}
			}
			else if ((object)type == typeof(ushort))
			{
				if (ushort.TryParse(text, out ushort result4))
				{
					mValue = result4;
				}
			}
			else if ((object)type == typeof(int))
			{
				if (int.TryParse(text, out int result5))
				{
					mValue = result5;
				}
			}
			else if ((object)type == typeof(uint))
			{
				if (uint.TryParse(text, out uint result6))
				{
					mValue = result6;
				}
			}
			else if ((object)type == typeof(float))
			{
				if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result7))
				{
					mValue = result7;
				}
			}
			else if ((object)type == typeof(double))
			{
				if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double result8))
				{
					mValue = result8;
				}
			}
			else if ((object)type == typeof(Vector2))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Vector2 vector = default(Vector2);
				if (parts.Length == 2 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.y))
				{
					mValue = vector;
				}
			}
			else if ((object)type == typeof(Vector3))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Vector3 vector2 = default(Vector3);
				if (parts.Length == 3 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector2.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector2.y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector2.z))
				{
					mValue = vector2;
				}
			}
			else if ((object)type == typeof(Vector4))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Vector4 vector3 = default(Vector4);
				if (parts.Length == 4 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.z) && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.w))
				{
					mValue = vector3;
				}
			}
			else if ((object)type == typeof(Quaternion))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Quaternion quaternion = default(Quaternion);
				if (parts.Length == 3)
				{
					Vector3 euler = default(Vector3);
					if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out euler.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out euler.y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out euler.z))
					{
						mValue = Quaternion.Euler(euler);
					}
				}
				else if (parts.Length == 4 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out quaternion.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out quaternion.y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out quaternion.z) && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out quaternion.w))
				{
					mValue = quaternion;
				}
			}
			else if ((object)type == typeof(Color))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Color color = default(Color);
				if (parts.Length == 4 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out color.r) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out color.g) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out color.b) && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out color.a))
				{
					mValue = color;
				}
			}
			else if ((object)type == typeof(Rect))
			{
				if (parts == null)
				{
					parts = text.Split(new char[1]
					{
						','
					});
				}
				Vector4 vector4 = default(Vector4);
				if (parts.Length == 4 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector4.x) && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector4.y) && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector4.z) && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out vector4.w))
				{
					mValue = new Rect(vector4.x, vector4.y, vector4.z, vector4.w);
				}
			}
			else
			{
				if (type.Implements(typeof(IDataNodeSerializable)))
				{
					IDataNodeSerializable dataNodeSerializable = (IDataNodeSerializable)type.Create();
					dataNodeSerializable.Deserialize(this);
					mValue = dataNodeSerializable;
					return false;
				}
				if (!type.IsSubclassOf(typeof(Component)))
				{
					bool flag = type.Implements(typeof(IList));
					bool flag2 = !flag && type.Implements(typeof(TList));
					mValue = ((!flag2 && !flag) ? type.Create() : type.Create(children.size));
					if (mValue == null)
					{
						UnityEngine.Debug.LogError("Unable to create a " + type);
						return false;
					}
					if (flag2)
					{
						TList tList = mValue as TList;
						Type genericArgument = type.GetGenericArgument();
						if ((object)genericArgument != null)
						{
							int num = 0;
							while (num < children.size)
							{
								DataNode dataNode = children[num];
								if (dataNode.name == "Add")
								{
									dataNode.ResolveValue(genericArgument);
									tList.Add(dataNode.mValue);
									children.RemoveAt(num);
								}
								else
								{
									num++;
								}
							}
						}
						else
						{
							UnityEngine.Debug.LogError("Unable to determine the element type of " + type);
						}
					}
					else if (flag)
					{
						IList list = mValue as IList;
						Type type2 = type.GetGenericArgument();
						if ((object)type2 == null)
						{
							type2 = type.GetElementType();
						}
						bool flag3 = list.Count == children.size;
						if ((object)type2 != null)
						{
							int num2 = 0;
							int num3 = 0;
							while (num2 < children.size)
							{
								DataNode dataNode2 = children[num2];
								if (dataNode2.name == "Add")
								{
									dataNode2.ResolveValue(type2);
									if (flag3)
									{
										list[num3] = dataNode2.mValue;
									}
									else
									{
										list.Add(dataNode2.mValue);
									}
									children.RemoveAt(num2);
								}
								else
								{
									num2++;
								}
								num3++;
							}
						}
						else
						{
							UnityEngine.Debug.LogError("Unable to determine the element type of " + type);
						}
					}
					else if (type.IsClass)
					{
						int num4 = 0;
						while (num4 < children.size)
						{
							DataNode dataNode3 = children[num4];
							if (mValue.SetSerializableField(dataNode3.name, dataNode3.value))
							{
								dataNode3.mValue = null;
								children.RemoveAt(num4);
							}
							else
							{
								num4++;
							}
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Unhandled type: " + type);
					}
				}
			}
			return true;
		}

		private static string GetNextLine(StreamReader reader)
		{
			string text = reader.ReadLine();
			while (text != null && text.Trim().StartsWith("//"))
			{
				text = reader.ReadLine();
				if (text == null)
				{
					return null;
				}
			}
			return text;
		}

		private static int CalculateTabs(string line)
		{
			if (line != null)
			{
				for (int i = 0; i < line.Length; i++)
				{
					if (line[i] != '\t')
					{
						return i;
					}
				}
			}
			return 0;
		}

		private static string Escape(string val)
		{
			if (!string.IsNullOrEmpty(val))
			{
				val = val.Replace("\n", "\\n");
				val = val.Replace("\t", "\\t");
			}
			return val;
		}

		private static string Unescape(string val)
		{
			if (!string.IsNullOrEmpty(val))
			{
				val = val.Replace("\\n", "\n");
				val = val.Replace("\\t", "\t");
			}
			return val;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		private static int HexToDecimal(char ch)
		{
			switch (ch)
			{
			case '0':
				return 0;
			case '1':
				return 1;
			case '2':
				return 2;
			case '3':
				return 3;
			case '4':
				return 4;
			case '5':
				return 5;
			case '6':
				return 6;
			case '7':
				return 7;
			case '8':
				return 8;
			case '9':
				return 9;
			case 'A':
			case 'a':
				return 10;
			case 'B':
			case 'b':
				return 11;
			case 'C':
			case 'c':
				return 12;
			case 'D':
			case 'd':
				return 13;
			case 'E':
			case 'e':
				return 14;
			case 'F':
			case 'f':
				return 15;
			default:
				return 15;
			}
		}

		private static Color32 ParseColor32(string text, int offset)
		{
			byte r = (byte)((HexToDecimal(text[offset]) << 4) | HexToDecimal(text[offset + 1]));
			byte g = (byte)((HexToDecimal(text[offset + 2]) << 4) | HexToDecimal(text[offset + 3]));
			byte b = (byte)((HexToDecimal(text[offset + 4]) << 4) | HexToDecimal(text[offset + 5]));
			byte a = (byte)((offset + 8 > text.Length) ? 255 : ((HexToDecimal(text[offset + 6]) << 4) | HexToDecimal(text[offset + 7])));
			return new Color32(r, g, b, a);
		}
	}
}
