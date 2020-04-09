using System.IO;

namespace TNet
{
	public interface IBinarySerializable
	{
		void Serialize(BinaryWriter writer);

		void Deserialize(BinaryReader reader);
	}
}
