using System.Reflection;

namespace TNet
{
    public struct CachedFunc
    {
        public byte id;

        public object obj;

        public MethodInfo func;

        public ParameterInfo[] parameters;
    }
}
