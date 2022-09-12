using UnityEngine;

namespace AABBCollider
{
	public class Collider
	{
		private int _type;
		private int _groupID;
		private int _order;
		private Shape _shape;
		private Vector2 _position;
		private AABBNode _node;

		public int getType()
		{
			return _type;
		}

		public int getGroupID()
		{
			return _groupID;
		}

		public int getOrder()
		{
			return _order;
		}

		public Shape getShape()
		{
			return _shape;
		}

		public Shape.ShapeType getShapeType()
		{
			return _shape.getType();
		}

		public Vector2 getPosition()
		{
			return _position;
		}

		public void setPosition(Vector2 position)
		{
			_position = position;
		}

		public AABB getAABB()
		{
			return _shape.getAABB(_position);
		}

		public AABBNode getNode()
		{
			return _node;
		}

		public void setNode(AABBNode node)
		{
			_node = node;
		}

		public static Collider create(int type,int group_id,int order,Shape shape,Vector2 position)
		{
			Collider collider = new Collider();
			collider.init(type,group_id,order,shape, position);
			return collider;
		}

		private void init(int type,int group_id,int order,Shape shape, Vector2 position)
		{
			_type = type;
			_groupID = group_id;
			_order = order;
			_shape = shape;
			_position = position;
			_node = null;
		}
	}
}
