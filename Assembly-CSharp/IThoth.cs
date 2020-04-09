using System.IO;

public interface IThoth
{
	int GetVersion();

	int GetCRC(bool read);

	void Write(BinaryWriter writer);

	void Read(BinaryReader reader);
}
