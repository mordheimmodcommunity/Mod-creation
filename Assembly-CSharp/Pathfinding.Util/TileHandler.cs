using Pathfinding.ClipperLib;
using Pathfinding.Poly2Tri;
using Pathfinding.Voxels;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Util
{
	public class TileHandler
	{
		public class TileType
		{
			private Int3[] verts;

			private int[] tris;

			private Int3 offset;

			private int lastYOffset;

			private int lastRotation;

			private int width;

			private int depth;

			private static readonly int[] Rotations = new int[16]
			{
				1,
				0,
				0,
				1,
				0,
				1,
				-1,
				0,
				-1,
				0,
				0,
				-1,
				0,
				-1,
				1,
				0
			};

			public int Width => width;

			public int Depth => depth;

			public TileType(Int3[] sourceVerts, int[] sourceTris, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1)
			{
				if (sourceVerts == null)
				{
					throw new ArgumentNullException("sourceVerts");
				}
				if (sourceTris == null)
				{
					throw new ArgumentNullException("sourceTris");
				}
				tris = new int[sourceTris.Length];
				for (int i = 0; i < tris.Length; i++)
				{
					tris[i] = sourceTris[i];
				}
				verts = new Int3[sourceVerts.Length];
				for (int j = 0; j < sourceVerts.Length; j++)
				{
					verts[j] = sourceVerts[j] + centerOffset;
				}
				offset = tileSize / 2f;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;
				for (int k = 0; k < sourceVerts.Length; k++)
				{
					verts[k] += offset;
				}
				lastRotation = 0;
				lastYOffset = 0;
				this.width = width;
				this.depth = depth;
			}

			public TileType(Mesh source, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}
				Vector3[] vertices = source.vertices;
				tris = source.triangles;
				verts = new Int3[vertices.Length];
				for (int i = 0; i < vertices.Length; i++)
				{
					verts[i] = (Int3)vertices[i] + centerOffset;
				}
				offset = tileSize / 2f;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;
				for (int j = 0; j < vertices.Length; j++)
				{
					verts[j] += offset;
				}
				lastRotation = 0;
				lastYOffset = 0;
				this.width = width;
				this.depth = depth;
			}

			public void Load(out Int3[] verts, out int[] tris, int rotation, int yoffset)
			{
				rotation = (rotation % 4 + 4) % 4;
				int num = rotation;
				rotation = (rotation - lastRotation % 4 + 4) % 4;
				lastRotation = num;
				verts = this.verts;
				int num2 = yoffset - lastYOffset;
				lastYOffset = yoffset;
				if (rotation != 0 || num2 != 0)
				{
					for (int i = 0; i < verts.Length; i++)
					{
						Int3 @int = verts[i] - offset;
						Int3 lhs = @int;
						lhs.y += num2;
						lhs.x = @int.x * Rotations[rotation * 4] + @int.z * Rotations[rotation * 4 + 1];
						lhs.z = @int.x * Rotations[rotation * 4 + 2] + @int.z * Rotations[rotation * 4 + 3];
						verts[i] = lhs + offset;
					}
				}
				tris = this.tris;
			}
		}

		[Flags]
		public enum CutMode
		{
			CutAll = 0x1,
			CutDual = 0x2,
			CutExtra = 0x4
		}

		private class Cut
		{
			public IntRect bounds;

			public Int2 boundsY;

			public bool isDual;

			public bool cutsAddedGeom;

			public List<IntPoint> contour;
		}

		private readonly RecastGraph _graph;

		private readonly int tileXCount;

		private readonly int tileZCount;

		private readonly Clipper clipper = (Clipper)(object)new Clipper(0);

		private int[] cached_int_array = new int[32];

		private readonly Dictionary<Int3, int> cached_Int3_int_dict = new Dictionary<Int3, int>();

		private readonly Dictionary<Int2, int> cached_Int2_int_dict = new Dictionary<Int2, int>();

		private readonly TileType[] activeTileTypes;

		private readonly int[] activeTileRotations;

		private readonly int[] activeTileOffsets;

		private readonly bool[] reloadedInBatch;

		private bool isBatching;

		private readonly VoxelPolygonClipper simpleClipper;

		public RecastGraph graph => _graph;

		public bool isValid => _graph != null && tileXCount == _graph.tileXCount && tileZCount == _graph.tileZCount;

		public TileHandler(RecastGraph graph)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}
			if (graph.GetTiles() == null)
			{
				Debug.LogWarning("Creating a TileHandler for a graph with no tiles. Please scan the graph before creating a TileHandler");
			}
			tileXCount = graph.tileXCount;
			tileZCount = graph.tileZCount;
			activeTileTypes = new TileType[tileXCount * tileZCount];
			activeTileRotations = new int[activeTileTypes.Length];
			activeTileOffsets = new int[activeTileTypes.Length];
			reloadedInBatch = new bool[activeTileTypes.Length];
			_graph = graph;
		}

		public void OnRecalculatedTiles(RecastGraph.NavmeshTile[] recalculatedTiles)
		{
			for (int i = 0; i < recalculatedTiles.Length; i++)
			{
				UpdateTileType(recalculatedTiles[i]);
			}
			bool flag = StartBatchLoad();
			for (int j = 0; j < recalculatedTiles.Length; j++)
			{
				ReloadTile(recalculatedTiles[j].x, recalculatedTiles[j].z);
			}
			if (flag)
			{
				EndBatchLoad();
			}
		}

		public int GetActiveRotation(Int2 p)
		{
			return activeTileRotations[p.x + p.y * _graph.tileXCount];
		}

		[Obsolete("Use the result from RegisterTileType instead")]
		public TileType GetTileType(int index)
		{
			throw new Exception("This method has been deprecated. Use the result from RegisterTileType instead.");
		}

		[Obsolete("Use the result from RegisterTileType instead")]
		public int GetTileTypeCount()
		{
			throw new Exception("This method has been deprecated. Use the result from RegisterTileType instead.");
		}

		public TileType RegisterTileType(Mesh source, Int3 centerOffset, int width = 1, int depth = 1)
		{
			return new TileType(source, new Int3(graph.tileSizeX, 1, graph.tileSizeZ) * (1000f * graph.cellSize), centerOffset, width, depth);
		}

		public void CreateTileTypesFromGraph()
		{
			RecastGraph.NavmeshTile[] tiles = graph.GetTiles();
			if (tiles == null)
			{
				return;
			}
			if (!isValid)
			{
				throw new InvalidOperationException("Graph tiles are invalid (number of tiles is not equal to width*depth of the graph). You need to create a new tile handler if you have changed the graph.");
			}
			for (int i = 0; i < graph.tileZCount; i++)
			{
				for (int j = 0; j < graph.tileXCount; j++)
				{
					RecastGraph.NavmeshTile tile = tiles[j + i * graph.tileXCount];
					UpdateTileType(tile);
				}
			}
		}

		private void UpdateTileType(RecastGraph.NavmeshTile tile)
		{
			int x = tile.x;
			int z = tile.z;
			Int3 lhs = (Int3)graph.GetTileBounds(x, z).min;
			Int3 tileSize = new Int3(graph.tileSizeX, 1, graph.tileSizeZ) * (1000f * graph.cellSize);
			lhs += new Int3(tileSize.x * tile.w / 2, 0, tileSize.z * tile.d / 2);
			lhs = -lhs;
			TileType tileType = new TileType(tile.verts, tile.tris, tileSize, lhs, tile.w, tile.d);
			int num = x + z * graph.tileXCount;
			activeTileTypes[num] = tileType;
			activeTileRotations[num] = 0;
			activeTileOffsets[num] = 0;
		}

		public bool StartBatchLoad()
		{
			if (isBatching)
			{
				return false;
			}
			isBatching = true;
			AstarPath.active.AddWorkItem(new AstarWorkItem((Func<bool, bool>)delegate
			{
				graph.StartBatchTileUpdate();
				return true;
			}));
			return true;
		}

		public void EndBatchLoad()
		{
			if (!isBatching)
			{
				throw new Exception("Ending batching when batching has not been started");
			}
			for (int i = 0; i < reloadedInBatch.Length; i++)
			{
				reloadedInBatch[i] = false;
			}
			isBatching = false;
			AstarPath.active.AddWorkItem(new AstarWorkItem((Func<bool, bool>)delegate
			{
				graph.EndBatchTileUpdate();
				return true;
			}));
		}

		private void CutPoly(Int3[] verts, int[] tris, ref Int3[] outVertsArr, ref int[] outTrisArr, out int outVCount, out int outTCount, Int3[] extraShape, Int3 cuttingOffset, Bounds realBounds, CutMode mode = CutMode.CutAll | CutMode.CutDual, int perturbate = -1)
		{
			//Discarded unreachable code: IL_08d4, IL_0904, IL_09b8, IL_09e8
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Expected O, but got Unknown
			//IL_0531: Unknown result type (might be due to invalid IL or missing references)
			//IL_054d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0569: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_070d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0712: Unknown result type (might be due to invalid IL or missing references)
			//IL_0781: Unknown result type (might be due to invalid IL or missing references)
			//IL_0786: Unknown result type (might be due to invalid IL or missing references)
			//IL_0794: Unknown result type (might be due to invalid IL or missing references)
			//IL_0799: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07aa: Expected O, but got Unknown
			//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07be: Unknown result type (might be due to invalid IL or missing references)
			//IL_07cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_084b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0852: Expected O, but got Unknown
			if (verts.Length == 0 || tris.Length == 0)
			{
				outVCount = 0;
				outTCount = 0;
				outTrisArr = new int[0];
				outVertsArr = new Int3[0];
				return;
			}
			if (perturbate > 10)
			{
				Debug.LogError("Too many perturbations aborting.\nThis may cause a tile in the navmesh to become empty. Try to see see if any of your NavmeshCut or NavmeshAdd components use invalid custom meshes.");
				outVCount = verts.Length;
				outTCount = tris.Length;
				outTrisArr = tris;
				outVertsArr = verts;
				return;
			}
			List<IntPoint> list = null;
			if (extraShape == null && (mode & CutMode.CutExtra) != 0)
			{
				throw new Exception("extraShape is null and the CutMode specifies that it should be used. Cannot use null shape.");
			}
			if ((mode & CutMode.CutExtra) != 0)
			{
				list = ListPool<IntPoint>.Claim(extraShape.Length);
				for (int i = 0; i < extraShape.Length; i++)
				{
					list.Add(new IntPoint((long)(extraShape[i].x + cuttingOffset.x), (long)(extraShape[i].z + cuttingOffset.z)));
				}
			}
			IntRect bounds = new IntRect(verts[0].x, verts[0].z, verts[0].x, verts[0].z);
			for (int j = 0; j < verts.Length; j++)
			{
				bounds = bounds.ExpandToContain(verts[j].x, verts[j].z);
			}
			List<NavmeshCut> list2 = (mode != CutMode.CutExtra) ? NavmeshCut.GetAllInRange(realBounds) : ListPool<NavmeshCut>.Claim();
			List<NavmeshAdd> allInRange = NavmeshAdd.GetAllInRange(realBounds);
			List<int> list3 = ListPool<int>.Claim();
			List<Cut> list4 = PrepareNavmeshCutsForCutting(list2, cuttingOffset, bounds, perturbate, allInRange.Count > 0);
			List<Int3> list5 = ListPool<Int3>.Claim(verts.Length * 2);
			List<int> list6 = ListPool<int>.Claim(tris.Length);
			Int3[] vbuffer = verts;
			int[] tbuffer = tris;
			if (list2.Count == 0 && allInRange.Count == 0 && (mode & ~(CutMode.CutAll | CutMode.CutDual)) == 0 && (mode & CutMode.CutAll) != 0)
			{
				CopyMesh(vbuffer, tbuffer, list5, list6);
			}
			else
			{
				List<IntPoint> list7 = ListPool<IntPoint>.Claim();
				Dictionary<TriangulationPoint, int> dictionary = new Dictionary<TriangulationPoint, int>();
				List<PolygonPoint> list8 = ListPool<PolygonPoint>.Claim();
				PolyTree val = (PolyTree)(object)new PolyTree();
				List<List<IntPoint>> intermediateResult = new List<List<IntPoint>>();
				Stack<Polygon> stack = new Stack<Polygon>();
				clipper.set_StrictlySimple(perturbate > -1);
				clipper.set_ReverseSolution(true);
				Int3[] array = null;
				Int3[] clipOut = null;
				Int2 @int = default(Int2);
				if (allInRange.Count > 0)
				{
					array = new Int3[7];
					clipOut = new Int3[7];
					Int3 int2 = (Int3)realBounds.extents;
					int x = int2.x;
					Int3 int3 = (Int3)realBounds.extents;
					@int = new Int2(x, int3.z);
				}
				int num = -1;
				int num2 = -3;
				Int3 int9 = default(Int3);
				Int3 int10 = default(Int3);
				while (true)
				{
					num2 += 3;
					while (num2 >= tbuffer.Length)
					{
						num++;
						num2 = 0;
						if (num >= allInRange.Count)
						{
							vbuffer = null;
							break;
						}
						if (vbuffer == verts)
						{
							vbuffer = null;
						}
						allInRange[num].GetMesh(cuttingOffset, ref vbuffer, out tbuffer);
					}
					if (vbuffer == null)
					{
						break;
					}
					Int3 int4 = vbuffer[tbuffer[num2]];
					Int3 int5 = vbuffer[tbuffer[num2 + 1]];
					Int3 int6 = vbuffer[tbuffer[num2 + 2]];
					if (VectorMath.IsColinearXZ(int4, int5, int6))
					{
						Debug.LogWarning("Skipping degenerate triangle.");
						continue;
					}
					IntRect a = new IntRect(int4.x, int4.z, int4.x, int4.z).ExpandToContain(int5.x, int5.z).ExpandToContain(int6.x, int6.z);
					int num3 = Math.Min(int4.y, Math.Min(int5.y, int6.y));
					int num4 = Math.Max(int4.y, Math.Max(int5.y, int6.y));
					list3.Clear();
					bool flag = false;
					for (int k = 0; k < list4.Count; k++)
					{
						int x2 = list4[k].boundsY.x;
						int y = list4[k].boundsY.y;
						if (IntRect.Intersects(a, list4[k].bounds) && y >= num3 && x2 <= num4 && (list4[k].cutsAddedGeom || num == -1))
						{
							Int3 int7 = int4;
							int7.y = x2;
							Int3 int8 = int4;
							int8.y = y;
							list3.Add(k);
							flag |= list4[k].isDual;
						}
					}
					if (list3.Count == 0 && (mode & CutMode.CutExtra) == 0 && (mode & CutMode.CutAll) != 0 && num == -1)
					{
						list6.Add(list5.Count);
						list6.Add(list5.Count + 1);
						list6.Add(list5.Count + 2);
						list5.Add(int4);
						list5.Add(int5);
						list5.Add(int6);
						continue;
					}
					list7.Clear();
					if (num == -1)
					{
						list7.Add(new IntPoint((long)int4.x, (long)int4.z));
						list7.Add(new IntPoint((long)int5.x, (long)int5.z));
						list7.Add(new IntPoint((long)int6.x, (long)int6.z));
					}
					else
					{
						array[0] = int4;
						array[1] = int5;
						array[2] = int6;
						int num5 = ClipAgainstRectangle(array, clipOut, new Int2(@int.x * 2, @int.y * 2));
						if (num5 == 0)
						{
							continue;
						}
						for (int l = 0; l < num5; l++)
						{
							list7.Add(new IntPoint((long)array[l].x, (long)array[l].z));
						}
					}
					dictionary.Clear();
					for (int m = 0; m < 16; m++)
					{
						if ((((int)mode >> m) & 1) == 0)
						{
							continue;
						}
						if (1 << m == 1)
						{
							CutAll(list7, list3, list4, val);
						}
						else if (1 << m == 2)
						{
							if (!flag)
							{
								continue;
							}
							CutDual(list7, list3, list4, flag, intermediateResult, val);
						}
						else if (1 << m == 4)
						{
							CutExtra(list7, list, val);
						}
						for (int n = 0; n < ((PolyNode)val).get_ChildCount(); n++)
						{
							PolyNode val2 = ((PolyNode)val).get_Childs()[n];
							List<IntPoint> contour = val2.get_Contour();
							List<PolyNode> childs = val2.get_Childs();
							if (childs.Count == 0 && contour.Count == 3 && num == -1)
							{
								for (int num6 = 0; num6 < 3; num6++)
								{
									IntPoint val3 = contour[num6];
									int x3 = (int)val3.X;
									IntPoint val4 = contour[num6];
									int9 = new Int3(x3, 0, (int)val4.Y);
									int9.y = SampleYCoordinateInTriangle(int4, int5, int6, int9);
									list6.Add(list5.Count);
									list5.Add(int9);
								}
								continue;
							}
							Polygon val5 = null;
							int num7 = -1;
							for (List<IntPoint> list9 = contour; list9 != null; list9 = ((num7 >= childs.Count) ? null : childs[num7].get_Contour()))
							{
								list8.Clear();
								for (int num8 = 0; num8 < list9.Count; num8++)
								{
									IntPoint val6 = list9[num8];
									double num9 = val6.X;
									IntPoint val7 = list9[num8];
									PolygonPoint val8 = (PolygonPoint)(object)new PolygonPoint(num9, (double)val7.Y);
									list8.Add(val8);
									IntPoint val9 = list9[num8];
									int x4 = (int)val9.X;
									IntPoint val10 = list9[num8];
									int10 = new Int3(x4, 0, (int)val10.Y);
									int10.y = SampleYCoordinateInTriangle(int4, int5, int6, int10);
									dictionary[(TriangulationPoint)(object)val8] = list5.Count;
									list5.Add(int10);
								}
								Polygon val11 = null;
								if (stack.Count > 0)
								{
									val11 = stack.Pop();
									val11.AddPoints((IEnumerable<PolygonPoint>)list8);
								}
								else
								{
									val11 = (Polygon)(object)new Polygon((IList<PolygonPoint>)list8);
								}
								if (num7 == -1)
								{
									val5 = val11;
								}
								else
								{
									val5.AddHole(val11);
								}
								num7++;
							}
							try
							{
								P2T.Triangulate(val5);
							}
							catch (PointOnEdgeException)
							{
								Debug.LogWarning("PointOnEdgeException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
								CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
								return;
							}
							catch
							{
								Debug.LogWarning("Exception, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
								CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
								return;
							}
							try
							{
								for (int num10 = 0; num10 < val5.get_Triangles().Count; num10++)
								{
									DelaunayTriangle val13 = val5.get_Triangles()[num10];
									list6.Add(dictionary[val13.Points._0]);
									list6.Add(dictionary[val13.Points._1]);
									list6.Add(dictionary[val13.Points._2]);
								}
							}
							catch (KeyNotFoundException)
							{
								Debug.LogWarning("KeyNotFoundException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
								CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
								return;
							}
							catch
							{
								Debug.LogWarning("KeyNotFoundException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
								CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
								return;
							}
							PoolPolygon(val5, stack);
						}
					}
				}
				ListPool<IntPoint>.Release(list7);
				ListPool<PolygonPoint>.Release(list8);
			}
			CompressMesh(list5, list6, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount);
			for (int num11 = 0; num11 < list2.Count; num11++)
			{
				list2[num11].UsedForCut();
			}
			ListPool<Int3>.Release(list5);
			ListPool<int>.Release(list6);
			ListPool<int>.Release(list3);
			for (int num12 = 0; num12 < list4.Count; num12++)
			{
				ListPool<IntPoint>.Release(list4[num12].contour);
			}
			ListPool<Cut>.Release(list4);
			ListPool<NavmeshCut>.Release(list2);
		}

		private static List<Cut> PrepareNavmeshCutsForCutting(List<NavmeshCut> navmeshCuts, Int3 cuttingOffset, IntRect bounds, int perturbate, bool anyNavmeshAdds)
		{
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			System.Random random = null;
			if (perturbate > 0)
			{
				random = new System.Random();
			}
			List<List<IntPoint>> list = ListPool<List<IntPoint>>.Claim();
			List<Cut> list2 = ListPool<Cut>.Claim();
			IntRect bounds3 = default(IntRect);
			for (int i = 0; i < navmeshCuts.Count; i++)
			{
				Bounds bounds2 = navmeshCuts[i].GetBounds();
				Int3 @int = (Int3)bounds2.min + cuttingOffset;
				Int3 int2 = (Int3)bounds2.max + cuttingOffset;
				IntRect a = new IntRect(@int.x, @int.z, int2.x, int2.z);
				if (!IntRect.Intersects(a, bounds) && !anyNavmeshAdds)
				{
					continue;
				}
				Int2 int3 = new Int2(0, 0);
				if (perturbate > 0)
				{
					int3.x = random.Next() % 6 * perturbate - 3 * perturbate;
					if (int3.x >= 0)
					{
						int3.x++;
					}
					int3.y = random.Next() % 6 * perturbate - 3 * perturbate;
					if (int3.y >= 0)
					{
						int3.y++;
					}
				}
				list.Clear();
				navmeshCuts[i].GetContour(list);
				for (int j = 0; j < list.Count; j++)
				{
					List<IntPoint> list3 = list[j];
					if (list3.Count == 0)
					{
						Debug.LogError("Zero Length Contour");
						continue;
					}
					IntPoint val = list3[0];
					int xmin = (int)val.X + cuttingOffset.x;
					IntPoint val2 = list3[0];
					int ymin = (int)val2.Y + cuttingOffset.z;
					IntPoint val3 = list3[0];
					int xmax = (int)val3.X + cuttingOffset.x;
					IntPoint val4 = list3[0];
					bounds3 = new IntRect(xmin, ymin, xmax, (int)val4.Y + cuttingOffset.z);
					for (int k = 0; k < list3.Count; k++)
					{
						IntPoint val5 = list3[k];
						val5.X += cuttingOffset.x;
						val5.Y += cuttingOffset.z;
						if (perturbate > 0)
						{
							val5.X += int3.x;
							val5.Y += int3.y;
						}
						bounds3 = bounds3.ExpandToContain((int)val5.X, (int)val5.Y);
						list3[k] = new IntPoint(val5.X, val5.Y);
					}
					Cut cut = new Cut();
					cut.boundsY = new Int2(@int.y, int2.y);
					cut.bounds = bounds3;
					cut.isDual = navmeshCuts[i].isDual;
					cut.cutsAddedGeom = navmeshCuts[i].cutsAddedGeom;
					cut.contour = list3;
					list2.Add(cut);
				}
			}
			ListPool<List<IntPoint>>.Release(list);
			return list2;
		}

		private static void PoolPolygon(Polygon polygon, Stack<Polygon> pool)
		{
			if (polygon.get_Holes() != null)
			{
				for (int i = 0; i < polygon.get_Holes().Count; i++)
				{
					polygon.get_Holes()[i].get_Points().Clear();
					polygon.get_Holes()[i].ClearTriangles();
					if (polygon.get_Holes()[i].get_Holes() != null)
					{
						polygon.get_Holes()[i].get_Holes().Clear();
					}
					pool.Push(polygon.get_Holes()[i]);
				}
			}
			polygon.ClearTriangles();
			if (polygon.get_Holes() != null)
			{
				polygon.get_Holes().Clear();
			}
			polygon.get_Points().Clear();
			pool.Push(polygon);
		}

		private static int SampleYCoordinateInTriangle(Int3 p1, Int3 p2, Int3 p3, Int3 p)
		{
			double num = (double)(p2.z - p3.z) * (double)(p1.x - p3.x) + (double)(p3.x - p2.x) * (double)(p1.z - p3.z);
			double num2 = ((double)(p2.z - p3.z) * (double)(p.x - p3.x) + (double)(p3.x - p2.x) * (double)(p.z - p3.z)) / num;
			double num3 = ((double)(p3.z - p1.z) * (double)(p.x - p3.x) + (double)(p1.x - p3.x) * (double)(p.z - p3.z)) / num;
			return (int)Math.Round(num2 * (double)p1.y + num3 * (double)p2.y + (1.0 - num2 - num3) * (double)p3.y);
		}

		private void CutAll(List<IntPoint> poly, List<int> intersectingCutIndices, List<Cut> cuts, PolyTree result)
		{
			clipper.Clear();
			((ClipperBase)clipper).AddPolygon(poly, (PolyType)0);
			for (int i = 0; i < intersectingCutIndices.Count; i++)
			{
				((ClipperBase)clipper).AddPolygon(cuts[intersectingCutIndices[i]].contour, (PolyType)1);
			}
			result.Clear();
			clipper.Execute((ClipType)2, result, (PolyFillType)1, (PolyFillType)1);
		}

		private void CutDual(List<IntPoint> poly, List<int> tmpIntersectingCuts, List<Cut> cuts, bool hasDual, List<List<IntPoint>> intermediateResult, PolyTree result)
		{
			clipper.Clear();
			((ClipperBase)clipper).AddPolygon(poly, (PolyType)0);
			for (int i = 0; i < tmpIntersectingCuts.Count; i++)
			{
				if (cuts[tmpIntersectingCuts[i]].isDual)
				{
					((ClipperBase)clipper).AddPolygon(cuts[tmpIntersectingCuts[i]].contour, (PolyType)1);
				}
			}
			clipper.Execute((ClipType)0, intermediateResult, (PolyFillType)0, (PolyFillType)1);
			clipper.Clear();
			if (intermediateResult != null)
			{
				for (int j = 0; j < intermediateResult.Count; j++)
				{
					((ClipperBase)clipper).AddPolygon(intermediateResult[j], (PolyType)(Clipper.Orientation(intermediateResult[j]) ? 1 : 0));
				}
			}
			for (int k = 0; k < tmpIntersectingCuts.Count; k++)
			{
				if (!cuts[tmpIntersectingCuts[k]].isDual)
				{
					((ClipperBase)clipper).AddPolygon(cuts[tmpIntersectingCuts[k]].contour, (PolyType)1);
				}
			}
			result.Clear();
			clipper.Execute((ClipType)2, result, (PolyFillType)0, (PolyFillType)1);
		}

		private void CutExtra(List<IntPoint> poly, List<IntPoint> extraClipShape, PolyTree result)
		{
			clipper.Clear();
			((ClipperBase)clipper).AddPolygon(poly, (PolyType)0);
			((ClipperBase)clipper).AddPolygon(extraClipShape, (PolyType)1);
			result.Clear();
			clipper.Execute((ClipType)0, result, (PolyFillType)0, (PolyFillType)1);
		}

		private int ClipAgainstRectangle(Int3[] clipIn, Int3[] clipOut, Int2 size)
		{
			int num = simpleClipper.ClipPolygon(clipIn, 3, clipOut, 1, 0, 0);
			if (num == 0)
			{
				return num;
			}
			num = simpleClipper.ClipPolygon(clipOut, num, clipIn, -1, size.x, 0);
			if (num == 0)
			{
				return num;
			}
			num = simpleClipper.ClipPolygon(clipIn, num, clipOut, 1, 0, 2);
			if (num == 0)
			{
				return num;
			}
			return simpleClipper.ClipPolygon(clipOut, num, clipIn, -1, size.y, 2);
		}

		private void CopyMesh(Int3[] vertices, int[] triangles, List<Int3> outVertices, List<int> outTriangles)
		{
			outTriangles.Capacity = Math.Max(outTriangles.Capacity, triangles.Length);
			outVertices.Capacity = Math.Max(outVertices.Capacity, triangles.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				outVertices.Add(vertices[i]);
			}
			for (int j = 0; j < triangles.Length; j++)
			{
				outTriangles.Add(triangles[j]);
			}
		}

		private void CompressMesh(List<Int3> vertices, List<int> triangles, ref Int3[] outVertices, ref int[] outTriangles, out int outVertexCount, out int outTriangleCount)
		{
			Dictionary<Int3, int> dictionary = cached_Int3_int_dict;
			dictionary.Clear();
			if (cached_int_array.Length < vertices.Count)
			{
				cached_int_array = new int[Math.Max(cached_int_array.Length * 2, vertices.Count)];
			}
			int[] array = cached_int_array;
			int num = 0;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (!dictionary.TryGetValue(vertices[i], out int value) && !dictionary.TryGetValue(vertices[i] + new Int3(0, 1, 0), out value) && !dictionary.TryGetValue(vertices[i] + new Int3(0, -1, 0), out value))
				{
					dictionary.Add(vertices[i], num);
					array[i] = num;
					vertices[num] = vertices[i];
					num++;
				}
				else
				{
					array[i] = value;
				}
			}
			outTriangleCount = triangles.Count;
			if (outTriangles == null || outTriangles.Length < outTriangleCount)
			{
				outTriangles = new int[outTriangleCount];
			}
			for (int j = 0; j < outTriangleCount; j++)
			{
				outTriangles[j] = array[triangles[j]];
			}
			outVertexCount = num;
			if (outVertices == null || outVertices.Length < outVertexCount)
			{
				outVertices = new Int3[outVertexCount];
			}
			for (int k = 0; k < outVertexCount; k++)
			{
				outVertices[k] = vertices[k];
			}
		}

		private void DelaunayRefinement(Int3[] verts, int[] tris, ref int vCount, ref int tCount, bool delaunay, bool colinear, Int3 worldOffset)
		{
			if (tCount % 3 != 0)
			{
				throw new ArgumentException("Triangle array length must be a multiple of 3");
			}
			Dictionary<Int2, int> dictionary = cached_Int2_int_dict;
			dictionary.Clear();
			for (int i = 0; i < tCount; i += 3)
			{
				if (!VectorMath.IsClockwiseXZ(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]))
				{
					int num = tris[i];
					tris[i] = tris[i + 2];
					tris[i + 2] = num;
				}
				dictionary[new Int2(tris[i], tris[i + 1])] = i + 2;
				dictionary[new Int2(tris[i + 1], tris[i + 2])] = i;
				dictionary[new Int2(tris[i + 2], tris[i])] = i + 1;
			}
			for (int j = 0; j < tCount; j += 3)
			{
				for (int k = 0; k < 3; k++)
				{
					if (!dictionary.TryGetValue(new Int2(tris[j + (k + 1) % 3], tris[j + k % 3]), out int value))
					{
						continue;
					}
					Int3 @int = verts[tris[j + (k + 2) % 3]];
					Int3 int2 = verts[tris[j + (k + 1) % 3]];
					Int3 int3 = verts[tris[j + (k + 3) % 3]];
					Int3 int4 = verts[tris[value]];
					@int.y = 0;
					int2.y = 0;
					int3.y = 0;
					int4.y = 0;
					bool flag = false;
					if (!VectorMath.RightOrColinearXZ(@int, int3, int4) || VectorMath.RightXZ(@int, int2, int4))
					{
						if (!colinear)
						{
							continue;
						}
						flag = true;
					}
					if (colinear && VectorMath.SqrDistancePointSegmentApproximate(@int, int4, int2) < 9f && !dictionary.ContainsKey(new Int2(tris[j + (k + 2) % 3], tris[j + (k + 1) % 3])) && !dictionary.ContainsKey(new Int2(tris[j + (k + 1) % 3], tris[value])))
					{
						tCount -= 3;
						int num2 = value / 3 * 3;
						tris[j + (k + 1) % 3] = tris[value];
						if (num2 != tCount)
						{
							tris[num2] = tris[tCount];
							tris[num2 + 1] = tris[tCount + 1];
							tris[num2 + 2] = tris[tCount + 2];
							dictionary[new Int2(tris[num2], tris[num2 + 1])] = num2 + 2;
							dictionary[new Int2(tris[num2 + 1], tris[num2 + 2])] = num2;
							dictionary[new Int2(tris[num2 + 2], tris[num2])] = num2 + 1;
							tris[tCount] = 0;
							tris[tCount + 1] = 0;
							tris[tCount + 2] = 0;
						}
						else
						{
							tCount += 3;
						}
						dictionary[new Int2(tris[j], tris[j + 1])] = j + 2;
						dictionary[new Int2(tris[j + 1], tris[j + 2])] = j;
						dictionary[new Int2(tris[j + 2], tris[j])] = j + 1;
					}
					else if (delaunay && !flag)
					{
						float num3 = Int3.Angle(int2 - @int, int3 - @int);
						float num4 = Int3.Angle(int2 - int4, int3 - int4);
						if (num4 > MathF.PI * 2f - 2f * num3)
						{
							tris[j + (k + 1) % 3] = tris[value];
							int num5 = value / 3 * 3;
							int num6 = value - num5;
							tris[num5 + (num6 - 1 + 3) % 3] = tris[j + (k + 2) % 3];
							dictionary[new Int2(tris[j], tris[j + 1])] = j + 2;
							dictionary[new Int2(tris[j + 1], tris[j + 2])] = j;
							dictionary[new Int2(tris[j + 2], tris[j])] = j + 1;
							dictionary[new Int2(tris[num5], tris[num5 + 1])] = num5 + 2;
							dictionary[new Int2(tris[num5 + 1], tris[num5 + 2])] = num5;
							dictionary[new Int2(tris[num5 + 2], tris[num5])] = num5 + 1;
						}
					}
				}
			}
		}

		private Vector3 Point2D2V3(TriangulationPoint p)
		{
			return new Vector3((float)p.X, 0f, (float)p.Y) * 0.001f;
		}

		private Int3 IntPoint2Int3(IntPoint p)
		{
			return new Int3((int)p.X, 0, (int)p.Y);
		}

		public void ClearTile(int x, int z)
		{
			if (!(AstarPath.active == null) && x >= 0 && z >= 0 && x < graph.tileXCount && z < graph.tileZCount)
			{
				AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
				{
					graph.ReplaceTile(x, z, new Int3[0], new int[0], worldSpace: false);
					activeTileTypes[x + z * graph.tileXCount] = null;
					GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
					context.QueueFloodFill();
					return true;
				}));
			}
		}

		public void ReloadInBounds(Bounds b)
		{
			Int2 tileCoordinates = graph.GetTileCoordinates(b.min);
			Int2 tileCoordinates2 = graph.GetTileCoordinates(b.max);
			IntRect a = new IntRect(tileCoordinates.x, tileCoordinates.y, tileCoordinates2.x, tileCoordinates2.y);
			a = IntRect.Intersection(a, new IntRect(0, 0, graph.tileXCount - 1, graph.tileZCount - 1));
			if (!a.IsValid())
			{
				return;
			}
			for (int i = a.ymin; i <= a.ymax; i++)
			{
				for (int j = a.xmin; j <= a.xmax; j++)
				{
					ReloadTile(j, i);
				}
			}
		}

		public void ReloadTile(int x, int z)
		{
			if (x >= 0 && z >= 0 && x < graph.tileXCount && z < graph.tileZCount)
			{
				int num = x + z * graph.tileXCount;
				if (activeTileTypes[num] != null)
				{
					LoadTile(activeTileTypes[num], x, z, activeTileRotations[num], activeTileOffsets[num]);
				}
			}
		}

		public void CutShapeWithTile(int x, int z, Int3[] shape, ref Int3[] verts, ref int[] tris, out int vCount, out int tCount)
		{
			if (isBatching)
			{
				throw new Exception("Cannot cut with shape when batching. Please stop batching first.");
			}
			int num = x + z * graph.tileXCount;
			if (x < 0 || z < 0 || x >= graph.tileXCount || z >= graph.tileZCount || activeTileTypes[num] == null)
			{
				verts = new Int3[0];
				tris = new int[0];
				vCount = 0;
				tCount = 0;
				return;
			}
			activeTileTypes[num].Load(out Int3[] verts2, out int[] tris2, activeTileRotations[num], activeTileOffsets[num]);
			Bounds tileBounds = graph.GetTileBounds(x, z);
			Int3 lhs = (Int3)tileBounds.min;
			lhs = -lhs;
			CutPoly(verts2, tris2, ref verts, ref tris, out vCount, out tCount, shape, lhs, tileBounds, CutMode.CutExtra);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] -= lhs;
			}
		}

		protected static T[] ShrinkArray<T>(T[] arr, int newLength)
		{
			newLength = Math.Min(newLength, arr.Length);
			T[] array = new T[newLength];
			if (newLength % 4 == 0)
			{
				for (int i = 0; i < newLength; i += 4)
				{
					array[i] = arr[i];
					array[i + 1] = arr[i + 1];
					array[i + 2] = arr[i + 2];
					array[i + 3] = arr[i + 3];
				}
			}
			else if (newLength % 3 == 0)
			{
				for (int j = 0; j < newLength; j += 3)
				{
					array[j] = arr[j];
					array[j + 1] = arr[j + 1];
					array[j + 2] = arr[j + 2];
				}
			}
			else if (newLength % 2 == 0)
			{
				for (int k = 0; k < newLength; k += 2)
				{
					array[k] = arr[k];
					array[k + 1] = arr[k + 1];
				}
			}
			else
			{
				for (int l = 0; l < newLength; l++)
				{
					array[l] = arr[l];
				}
			}
			return array;
		}

		public void LoadTile(TileType tile, int x, int z, int rotation, int yoffset)
		{
			if (tile == null)
			{
				throw new ArgumentNullException("tile");
			}
			if (!(AstarPath.active == null))
			{
				int index = x + z * graph.tileXCount;
				rotation %= 4;
				if (!isBatching || !reloadedInBatch[index] || activeTileOffsets[index] != yoffset || activeTileRotations[index] != rotation || activeTileTypes[index] != tile)
				{
					reloadedInBatch[index] |= isBatching;
					activeTileOffsets[index] = yoffset;
					activeTileRotations[index] = rotation;
					activeTileTypes[index] = tile;
					AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
					{
						if (activeTileOffsets[index] != yoffset || activeTileRotations[index] != rotation || activeTileTypes[index] != tile)
						{
							return true;
						}
						GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
						tile.Load(out Int3[] verts, out int[] tris, rotation, yoffset);
						Bounds tileBounds = graph.GetTileBounds(x, z, tile.Width, tile.Depth);
						Int3 lhs = (Int3)tileBounds.min;
						lhs = -lhs;
						Int3[] outVertsArr = null;
						int[] outTrisArr = null;
						CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out int outVCount, out int outTCount, null, lhs, tileBounds);
						DelaunayRefinement(outVertsArr, outTrisArr, ref outVCount, ref outTCount, delaunay: true, colinear: false, -lhs);
						if (outTCount != outTrisArr.Length)
						{
							outTrisArr = ShrinkArray(outTrisArr, outTCount);
						}
						if (outVCount != outVertsArr.Length)
						{
							outVertsArr = ShrinkArray(outVertsArr, outVCount);
						}
						int w = (rotation % 2 != 0) ? tile.Depth : tile.Width;
						int d = (rotation % 2 != 0) ? tile.Width : tile.Depth;
						graph.ReplaceTile(x, z, w, d, outVertsArr, outTrisArr, worldSpace: false);
						GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
						context.QueueFloodFill();
						return true;
					}));
				}
			}
		}
	}
}
