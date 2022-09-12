using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AABBCollider
{
    public class AABBTest : MonoBehaviour
    {
        private AABBTestShape[] _shapeList;
        private InputModule_PC _inputModule;

		    private AABBTestShape _draggingShape;
		    private Vector2 _lastDragPosition;

		    private AABBTree _tree;

        void Awake()
		    {
            _shapeList = GetComponentsInChildren<AABBTestShape>(true);
            _inputModule = InputModule_PC.create();
			      _tree = new AABBTree();

			      foreach(AABBTestShape shape in _shapeList)
			      {
                shape.createCollider();
                _tree.insertObject(shape.getCollider());
			      }
		    }

		    private void Update()
		    {
            if( _inputModule.isTouchDown())
			      {
				_draggingShape = pickShape(_inputModule.getTouchPosition());
				RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, _inputModule.getTouchPosition(), Camera.main, out _lastDragPosition);
			}
			else if( _inputModule.isTouchDrag() && _draggingShape != null)
			{
				Vector2 curPosition;
				RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, _inputModule.getTouchPosition(), Camera.main, out curPosition);

				Vector2 delta = curPosition - _lastDragPosition;
				RectTransform rtShape = _draggingShape.transform as RectTransform;

				rtShape.anchoredPosition += delta;

				_lastDragPosition = curPosition;

				_draggingShape.updateCollider();
				_tree.updateObject(_draggingShape.getCollider());
			}
		}

		AABBTestShape pickShape(Vector2 position)
		{
			Vector3 rootScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

			Vector3 pickWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, rootScreenPosition.z));

			foreach(AABBTestShape shape in _shapeList)
			{
				Vector3 localPosition = shape.transform.InverseTransformPoint(pickWorldPosition);

				if( shape.type == Shape.ShapeType.Circle)
				{
					Vector2 diff = ((Vector2)localPosition) - shape.offset;
					if( diff.magnitude <= shape.radius)
					{
						return shape;
					}
				}
				else if( shape.type == Shape.ShapeType.Polygon)
				{
					Vector2 localFromOffset = (Vector2)localPosition - shape.offset;
					if( localFromOffset.x >= -shape.extent.x &&
						localFromOffset.x <= +shape.extent.x &&
						localFromOffset.y >= -shape.extent.y &&
						localFromOffset.y <= +shape.extent.y)
					{
						return shape;
					}
				}
			}

			return null;
		}

		private void OnDrawGizmos()
		{
			if( _tree == null)
			{
				return;
			}

			drawNodes();
			drawCollisionStates();
		}

		private void drawCollisionStates()
		{
			List<Collider> colList = new List<Collider>();
			foreach(AABBTestShape shape in _shapeList)
			{
				colList.Clear();
				_tree.queryOverlaps(shape.getCollider(), colList);
				if( colList.Count == 0)
				{
					continue;
				}

				foreach(Collider col in colList)
				{
					if( CollisionTest.test( shape.getCollider(), col))
					{
						shape.drawWireShape(Color.red);
						break;
					}
				}

			}
		}

		private void drawNodes()
		{
			AABBNode node = _tree.getRoot();
			drawNode(node);
		}

		private void drawNode(AABBNode node)
		{
			if( node == null)
			{
				return;
			}

			Vector2 position = node.aabb.CenterPosition;
			Vector2 size = node.aabb.Size;

			GizmoExtension.drawBoxLine(transform, position, size / 2, Color.black);

			drawNode(node.left);
			drawNode(node.right);
		}

	}
}
