namespace TNet
{
	public class Player
	{
		public const int version = 11;

		protected static int mPlayerCounter = 0;

		public int id = 1;

		public string name = "Guest";

		public object data;

		private static DataNode mDummy = new DataNode("Version", 11);

		public DataNode dataNode
		{
			get
			{
				DataNode dataNode = data as DataNode;
				return dataNode ?? mDummy;
			}
		}

		public Player()
		{
		}

		public Player(string playerName)
		{
			name = playerName;
		}
	}
}
