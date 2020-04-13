namespace Pathfinding
{
    public class PathNode
    {
        private const uint CostMask = 268435455u;

        private const int Flag1Offset = 28;

        private const uint Flag1Mask = 268435456u;

        private const int Flag2Offset = 29;

        private const uint Flag2Mask = 536870912u;

        public GraphNode node;

        public PathNode parent;

        public ushort pathID;

        private uint flags;

        private uint g;

        private uint h;

        public uint cost
        {
            get
            {
                return flags & 0xFFFFFFF;
            }
            set
            {
                flags = (uint)(((int)flags & -268435456) | (int)value);
            }
        }

        public bool flag1
        {
            get
            {
                return (flags & 0x10000000) != 0;
            }
            set
            {
                flags = (uint)(((int)flags & -268435457) | (value ? 268435456 : 0));
            }
        }

        public bool flag2
        {
            get
            {
                return (flags & 0x20000000) != 0;
            }
            set
            {
                flags = (uint)(((int)flags & -536870913) | (value ? 536870912 : 0));
            }
        }

        public uint G
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
            }
        }

        public uint H
        {
            get
            {
                return h;
            }
            set
            {
                h = value;
            }
        }

        public uint F => g + h;
    }
}
