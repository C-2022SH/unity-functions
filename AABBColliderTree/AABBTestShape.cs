using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AABBCollider
{
    public class AABBTestShape : MonoBehaviour
    {
        public Shape.ShapeType type = Shape.ShapeType.Polygon;
        public Vector2 offset = Vector2.zero;
        public Vector2 extent = Vector2.one;
        public float radius = 1;

        private Collider _collider;

		public Collider getCollider()
		{
			return _collider;
		}

        public void createCollider()
		{
            Shape shape = createShape();
			_collider = Collider.create(MapBoxDefine.ColliderType.icon,GetInstanceID(), 0, shape, transform.localPosition);
		}

		public void updateCollider()
		{
			_collider.setPosition(transform.localPosition);
		}

        private Shape createShape()
		{
            if( type == Shape.ShapeType.Circle)
			{
				return CircleShape.create(radius);
			}
            else if( type == Shape.ShapeType.Polygon)
			{
				return PolygonShape.createBox(offset, extent);
			}

			return null;
		}

		public void drawWireShape(Color color)
		{
			if (type == Shape.ShapeType.Polygon)
			{
				GizmoExtension.drawBoxLine(transform, offset, extent, color);
			}
			else
			{
				GizmoExtension.drawCircleLine(transform, offset, radius, color);
			}
		}
	}

}
