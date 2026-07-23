using System.Collections.Generic;
using System.Collections;
using Utilities.CustomDebugging;

namespace Utilities.DataStructures
{
    public class CustomGrid<T> : IEnumerable<T>
    {
        private int m_width;
        private int m_height;

        private T[,] m_values;

        public CustomGrid(int width=1, int height=1, T defaultValue = default(T))
        {
            this.m_width = width;
            this.m_height = height;

            CreateGrid();
            SetValue(defaultValue);
        }

        private void CreateGrid()
        {
            if (m_width <= 0)
            {
                CustomDebugger.LogError($"Width error.", DebugLog.Model);
            }
            if (m_height <= 0)
            {
                CustomDebugger.LogError($"Height error.");
            }

            m_values = new T[m_width, m_height];
        }

        public void GetSize(out int width, out int height)
        {
            width = this.m_width;
            height = this.m_height;
        }

        public void SetValue(int x, int y, T value)
        {
            CheckIndexes(x, y);
            m_values[x, y] = value;
        }

        public void SetValue(T value)
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                    m_values[x, y] = value;
            }
        }

        public T GetValue(int x, int y)
        {
            CheckIndexes(x, y);
            return m_values[x, y];
        }

        private bool CheckIndexes(int x, int y)
        {
            if (x < 0 || x > m_width - 1)
            {
                CustomDebugger.LogError($"Wrong index x:{x}", DebugLog.Model);
                return false;
            }
            if (y < 0 || y > m_height - 1)
            {
                CustomDebugger.LogError($"Wrong index y:{y}", DebugLog.Model);
                return false;
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int x = 0; x < m_width; x++)
                for (int y = 0; y < m_height; y++)
                    yield return m_values[x, y];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Compare operator ==

        public static bool operator ==(CustomGrid<T> a, CustomGrid<T> b)
        {
            // Same reference or both null
            if (ReferenceEquals(a, b))
                return true;

            // One is null
            if (a is null || b is null)
                return false;

            // Compare sizes
            if (a.m_width != b.m_width || a.m_height != b.m_height)
                return false;

            // Compare all values
            for (int x = 0; x < a.m_width; x++)
            {
                for (int y = 0; y < a.m_height; y++)
                {
                    if (!EqualityComparer<T>.Default.Equals(a.m_values[x, y], b.m_values[x, y]))
                        return false;
                }
            }

            return true;
        }

        public static bool operator !=(CustomGrid<T> a, CustomGrid<T> b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomGrid<T> other)
                return this == other;

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 31 + m_width.GetHashCode();
            hash = hash * 31 + m_height.GetHashCode();

            foreach (var value in m_values)
            {
                if (value != null)
                    hash = hash * 31 + value.GetHashCode();
            }

            return hash;
        }

        #endregion
    }

}

