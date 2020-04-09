namespace TNet
{
	public interface IDataNodeSerializable
	{
		void Serialize(DataNode node);

		void Deserialize(DataNode node);
	}
}
