using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AABBCollider
{
    public struct AABB
    {
        private Vector2 _centerPosition;
        public Vector2 CenterPosition
        {
            get { return _centerPosition; }
            set {
                _centerPosition = value;
                _min = _centerPosition - _size * 0.5f;
                _max = _centerPosition + _size * 0.5f;
            }
        }

        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set {
                _size = value;
                _min = _centerPosition - _size * 0.5f;
                _max = _centerPosition + _size * 0.5f;
            }
        }

        private float _surfaceArea;
        private Vector2 _min;
        private Vector3 _max;

        public AABB(Vector2 position, float radius)
        {
            if (radius < 0)
            {
                throw new Exception("negative radius not allowed");
            }

            _centerPosition = position;
            _size = new Vector2(radius * 2f, radius * 2f);
            _surfaceArea = calculateSurfaceArea(_size.x, _size.y);
            _min = _centerPosition - _size * 0.5f;
            _max = _centerPosition + _size * 0.5f;
        }

        public AABB(Vector2 position, Vector2 size)
        {
            _centerPosition = position;
            _size = size;
            _surfaceArea = calculateSurfaceArea(_size.x,_size.y);
            _min = _centerPosition - _size * 0.5f;
            _max = _centerPosition + _size * 0.5f;
        }

        public AABB(float minX, float minY, float maxX, float maxY)
        {
            _centerPosition = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            _size = new Vector2(maxX - minX, maxY - minY);
            _surfaceArea = calculateSurfaceArea(_size.x,_size.y);
            _min = _centerPosition - _size * 0.5f;
            _max = _centerPosition + _size * 0.5f;
        }

        public AABB(AABB aabb,Vector2 position)
		{
            _centerPosition = aabb._centerPosition + position;
            _size = aabb._size;
            _surfaceArea = aabb._surfaceArea;
            _min = _centerPosition - _size * 0.5f;
            _max = _centerPosition + _size * 0.5f;
        }

        public void reset()
        {
            _centerPosition = Vector2.zero;
            _size = Vector2.zero;
            _surfaceArea = 0f;
        }

        // 집합연산
        public bool overlaps(AABB other)
        {
            return _max.x > other._min.x &&
                   _min.x < other._max.x &&
                   _max.y > other._min.y &&
                   _min.y < other._max.y;
        }

        public bool contains(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return otherMin.x >= thisMin.x &&
                    otherMax.x <= thisMax.x &&
                    otherMin.y >= thisMin.y &&
                    otherMax.y <= thisMax.y;
        }

        public AABB union(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return new AABB(Mathf.Min(thisMin.x, otherMin.x), Mathf.Min(thisMin.y, otherMin.y),
                        Mathf.Max(thisMax.x, otherMax.x), Mathf.Max(thisMax.y, otherMax.y));
        }

        public AABB intersection(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return new AABB(Mathf.Max(thisMin.x, otherMin.x), Mathf.Max(thisMin.y, otherMin.y),
                        Mathf.Min(thisMax.x, otherMax.x), Mathf.Min(thisMax.y, otherMax.y));
        }

        private static float calculateSurfaceArea(float size_x, float size_y)
        {
            return size_x * size_y;
        }

        public float getSurfaceArea()
        {
            return _surfaceArea;
        }
    }
}
