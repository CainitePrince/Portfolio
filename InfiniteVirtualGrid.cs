using System;
using System.Collections.Generic;
using TestingTaskFramework;
using VRageMath;

namespace TestingTask
{
    internal sealed class InfiniteVirtualGrid
    {
        /// <summary>
        /// Number of buckets that objects will be sorted into
        /// </summary>
        private const int NUM_BUCKETS = 65536;

        /// <summary>
        /// Size of each virtual grid cell
        /// </summary>
        private const int CELLSIZE = 50;

        /// <summary>
        /// Buckets that the objects will be sorted into.
        /// </summary>
        private LinkedList<WorldObject>[] m_buckets = new LinkedList<WorldObject>[NUM_BUCKETS]; 

        /// <summary>
        /// List of moving objects
        /// </summary>
        private List<WorldObject> m_movingObjects = new List<WorldObject>();

        /// <summary>
        /// Number of items stored by the grid.
        /// </summary>
        private int m_count = 0;

        /// <summary>
        /// Number of moving objects.
        /// </summary>
        internal int MovingObjectsCount { get { return m_movingObjects.Count; } }

        /// <summary>
        /// Number of static objects.
        /// </summary>
        internal int StaticObjectsCount { get { return m_count; } }

        /// <summary>
        /// Add an object to the grid
        /// </summary>
        /// <param name="obj">Object to add</param>
        internal void Add(WorldObject obj)
        {
            if (obj.NeedsUpdate || obj.LinearVelocity.LengthSquared() > 0.0f)
            {
                m_movingObjects.Add(obj);
            }
            else
            {
                int index = GetHashBucketIndex(obj.Position);

                if (m_buckets[index] == null)
                {
                    m_buckets[index] = new LinkedList<WorldObject>();
                }

                m_buckets[index].AddFirst(obj);

                ++m_count;
            }
        }

        /// <summary>
        /// Remove an object from the grid
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void Remove(WorldObject obj) 
        {
            if (obj.LinearVelocity.LengthSquared() > 0 || obj.NeedsUpdate)
            {
                bool removed = m_movingObjects.Remove(obj);

                if (!removed)
                {
                    throw new InvalidOperationException("Moving object to remove not found.");
                }
            }
            else
            {
                int index = GetHashBucketIndex(obj.Position);

                if (m_buckets[index] != null)
                {
                    bool removed = m_buckets[index].Remove(obj);

                    if (!removed)
                    {
                        throw new InvalidOperationException("Object to remove not found.");
                    }

                    --m_count;
                }
                else
                {
                    throw new InvalidOperationException("Bucket has no items in it.");
                }
            }
        }

        /// <summary>
        /// Reset the grid, clear the objects from it. 
        /// </summary>
        internal void Clear()
        {
            m_movingObjects.Clear();

            for (int i = 0; i < NUM_BUCKETS; ++i)
            {
                if (m_buckets[i] != null)
                {
                    m_buckets[i].Clear();
                }
            }
            m_count = 0;
        }

        /// <summary>
        /// Check which objects in the grid overlap with a bounding box 
        /// </summary>
        /// <param name="box">Box to test against</param>
        /// <param name="resultList">Objects that overlap with the box</param>
        internal void Query(ref BoundingBox box, List<WorldObject> resultList)
        {
            if (!box.Min.IsValid() || !box.Min.IsValid())
            {
                return;
            }

            int minX = GetGridIndex(box.Min.X) - 1;
            int maxX = GetGridIndex(box.Max.X) + 1;
            int minY = GetGridIndex(box.Min.Z) - 1;
            int maxY = GetGridIndex(box.Max.Z) + 1;

            for (int y = minY; y <= maxY; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    int index = GetHashBucketIndex(x, y);

                    var objects = m_buckets[index];

                    if (objects != null)
                    {
                        foreach (var obj in objects)
                        {
                            if (obj.BoundingBox.Contains(box) != ContainmentType.Disjoint)
                                resultList.Add(obj);
                        }
                    }
                }
            }

            for (int i = 0; i < m_movingObjects.Count; ++i)
            {
                var obj = m_movingObjects[i];

                if (obj.BoundingBox.Contains(box) != ContainmentType.Disjoint)
                    resultList.Add(obj);
            }

        }

        /// <summary>
        /// Check which cell an object falls into along one axis
        /// </summary>
        /// <param name="dimension">Position of the object along one axis</param>
        /// <returns>Cell index</returns>
        private static int GetGridIndex(float axis)
        {
            int d = (int)Math.Floor(axis / CELLSIZE);

            return d;
        }

        /// <summary>
        /// Convert object position into bucket index
        /// </summary>
        /// <param name="position">Position of object</param>
        /// <returns>Index of the bucket</returns>
        private static int GetHashBucketIndex(Vector3 position)
        {
            int x = GetGridIndex(position.X);
            int y = GetGridIndex(position.Z);

            return GetHashBucketIndex(x, y);
        }

        /// <summary>
        /// Convert cell index into bucket index
        /// </summary>
        /// <param name="x">Cell index along the x-axis</param>
        /// <param name="y">Cell index along the y-axis</param>
        /// <returns>Index of the bucket</returns>
        private static int GetHashBucketIndex(int x, int y)
        {
            const int h1 = 39916801;
            const int h2 = 479001599;

            int n = h1 * x + h2 * y;
            n = n % NUM_BUCKETS;
            
            if (n < 0) n += NUM_BUCKETS;
            
            return n;
        }
    }
}
