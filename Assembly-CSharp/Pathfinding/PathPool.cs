using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public static class PathPool
    {
        private static readonly Dictionary<Type, Stack<Path>> pool = new Dictionary<Type, Stack<Path>>();

        private static readonly Dictionary<Type, int> totalCreated = new Dictionary<Type, int>();

        public static void Pool(Path path)
        {
            lock (pool)
            {
                if (path.pooled)
                {
                    throw new ArgumentException("The path is already pooled.");
                }
                if (!pool.TryGetValue(path.GetType(), out Stack<Path> value))
                {
                    value = new Stack<Path>();
                    pool[path.GetType()] = value;
                }
                path.pooled = true;
                path.OnEnterPool();
                value.Push(path);
            }
        }

        public static int GetTotalCreated(Type type)
        {
            if (totalCreated.TryGetValue(type, out int value))
            {
                return value;
            }
            return 0;
        }

        public static int GetSize(Type type)
        {
            if (pool.TryGetValue(type, out Stack<Path> value))
            {
                return value.Count;
            }
            return 0;
        }

        public static T GetPath<T>() where T : Path, new()
        {
            //Discarded unreachable code: IL_00c5
            lock (pool)
            {
                T val;
                if (pool.TryGetValue(typeof(T), out Stack<Path> value) && value.Count > 0)
                {
                    val = (value.Pop() as T);
                }
                else
                {
                    val = new T();
                    if (!totalCreated.ContainsKey(typeof(T)))
                    {
                        totalCreated[typeof(T)] = 0;
                    }
                    Dictionary<Type, int> dictionary;
                    Dictionary<Type, int> dictionary2 = dictionary = totalCreated;
                    Type typeFromHandle;
                    Type key = typeFromHandle = typeof(T);
                    int num = dictionary[typeFromHandle];
                    dictionary2[key] = num + 1;
                }
                val.pooled = false;
                val.Reset();
                return val;
            }
        }
    }
    [Obsolete("Generic version is now obsolete to trade an extremely tiny performance decrease for a large decrease in boilerplate for Path classes")]
    public static class PathPool<T> where T : Path, new()
    {
        public static void Recycle(T path)
        {
            PathPool.Pool(path);
        }

        public static void Warmup(int count, int length)
        {
            ListPool<GraphNode>.Warmup(count, length);
            ListPool<Vector3>.Warmup(count, length);
            Path[] array = new Path[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = GetPath();
                array[i].Claim(array);
            }
            for (int j = 0; j < count; j++)
            {
                array[j].Release(array);
            }
        }

        public static int GetTotalCreated()
        {
            return PathPool.GetTotalCreated(typeof(T));
        }

        public static int GetSize()
        {
            return PathPool.GetSize(typeof(T));
        }

        [Obsolete("Use PathPool.GetPath<T> instead of PathPool<T>.GetPath")]
        public static T GetPath()
        {
            return PathPool.GetPath<T>();
        }
    }
}
