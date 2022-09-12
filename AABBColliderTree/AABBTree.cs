using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AABBCollider
{
    public class AABBNode
    {
        public AABB aabb;
        public AABBNode parent;
        public AABBNode left;
        public AABBNode right;
        public Collider collider;

        public AABBNode()
        {
            aabb = new AABB();
        }

        public bool isLeaf()
        {
            return left == null;
        }

        public void reset()
        {
            resetAABB();
            resetFamily();
        }

        public void resetAABB()
        {
            aabb.reset();
        }

        public void resetFamily()
        {
            parent = left = right = null;
        }
    }

    public class AABBTree
    {
        // ================================================== //
        // ||             여기서부터 진짜 트리!!!             || //
        // ================================================== //
        private AABBNode _root;
        private List<AABBNode> _potentialNodeList = new List<AABBNode>();   // AABB 박스가 충돌
        private List<Collider> _collidedColliderList = new List<Collider>();    // 물체가 진짜로 충돌
        public List<Collider> CollidedColliderList
        {
            get { return _collidedColliderList;}
        }

        public AABBNode getRoot()
        {
            return _root;
        }

        public void resetTree()
        {
            _potentialNodeList.Clear();
            _collidedColliderList.Clear();
        }

        public void insertObject(Collider collider)
        {
            AABBNode node = allocateNode();
            node.aabb = collider.getAABB();
            node.collider = collider;
            collider.setNode(node);
            // for debugging purpose
            // node.setName(collider.gameObject.name);

            insertLeaf(node);
        }

        public void removeObject(Collider collider)
        {
            removeLeaf(collider.getNode());
            deallocateNode(collider.getNode());
        }

        public void updateObject(Collider collider)
		    {
            updateLeaf(collider.getNode(), collider.getAABB());
		    }

        private void insertLeaf(AABBNode leafNode)
        {
            // 진짜로 새로운 애를 넣고 있는지 확인하기
            if (leafNode.parent != null || leafNode.left != null || leafNode.right != null)
                return;

            if (_root == null)
            {
                _root = leafNode;
                return;
            }

            AABBNode rootNode = _root;

            // 자리찾기
            while (!rootNode.isLeaf())
            {
                AABBNode leftNode = rootNode.left;
                AABBNode rightNode = rootNode.right;
                AABB mergedAABB = rootNode.aabb.union(leafNode.aabb);

                float newParentNodeCost = 2f * mergedAABB.getSurfaceArea();
                float minimumPushDownCost = 2f * (mergedAABB.getSurfaceArea() - rootNode.aabb.getSurfaceArea());

                // 왼오 비용계산,,
                float costLeft = leafNode.aabb.union(leftNode.aabb).getSurfaceArea() + minimumPushDownCost;
                float costRight = leafNode.aabb.union(rightNode.aabb).getSurfaceArea() + minimumPushDownCost;

                if (!leftNode.isLeaf())
                    costLeft -= leftNode.aabb.getSurfaceArea();

                if (!rightNode.isLeaf())
                    costRight -= rightNode.aabb.getSurfaceArea();

                // 여기서 새로운 parent node 를 만드는 게 더 비용이 싸다면!! 여기다가 새로운 부모를 만들고 leaf 를 붙인당
                if (newParentNodeCost < costLeft && newParentNodeCost < costRight)
                    break;

                // 아니라면 더 싼 곳으로 내려가자
                if (costLeft < costRight)
                    rootNode = rootNode.left;
                else
                    rootNode = rootNode.right;
            }

            // 그렇게 정해진 자리에,, 끼워넣어요
            AABBNode leafSibling = rootNode;            // 가장 값이 쌌던 곳이 leaf sibling 이 되어요
            AABBNode oldParent = leafSibling.parent;
            AABBNode newParent = allocateNode();

            newParent.parent = oldParent;               // 새로운 부모의 부모는 leaf 의 부모,, 갖다 붙이기
            newParent.aabb = leafNode.aabb.union(leafSibling.aabb);
            newParent.left = leafSibling;
            newParent.right = leafNode;

            leafNode.parent = newParent;
            leafSibling.parent = newParent;

            if (oldParent == null)
            {
                // old parent 가 root 였다는 뜻
                _root = newParent;
            }
            else
            {
                // 트리 구조 만드는 작업 마무리
                if (oldParent.left == leafSibling)
                {
                    oldParent.left = newParent;
                }
                else
                {
                    oldParent.right = newParent;
                }
            }

            fixUpwardTree(leafNode.parent);
        }

        public void removeLeaf(AABBNode leafNode)
        {
            if (leafNode == _root)
            {
                // 얘가 루트면 그냥 지우고 끝
                _root = null;
                return;
            }

            // 자자 가족 정리~~
            AABBNode parentNode = leafNode.parent;
            AABBNode grandParentNode = parentNode.parent;

            AABBNode siblingNode = parentNode.left == leafNode ? parentNode.right : parentNode.left;

            if (grandParentNode == null)
            {
                // 할아버지가 없다면~~ 그럼 사실상 나랑 내 자매 이렇게 둘만 있었던 거
                // if we have no gradparent then the parent is the root and so our sibling becomes the root and ahs it's parent removed (by RG)
                _root = siblingNode;
                siblingNode.parent = null;
                deallocateNode(parentNode);
            }
            else
            {
                // 할아버지가 있다면, 나를 지우고 내 자매를 할아버지의 자식으로 넣어 준다
                if (grandParentNode.left == parentNode)
                    grandParentNode.left = siblingNode;
                else
                    grandParentNode.right = siblingNode;

                siblingNode.parent = grandParentNode;
                deallocateNode(parentNode);

                fixUpwardTree(grandParentNode);
            }

            leafNode.parent = null;
        }

        public void updateLeaf(AABBNode node, AABB newAABB)
        {
            if (node == null)
                return;

            if (node.aabb.contains(newAABB))
                // 이미 포함되어 있어서 뭐,, 할 게 없음
                return;

            // aabb 쓱 갈아끼우기
            removeLeaf(node);
            node.aabb = newAABB;
            insertLeaf(node);
        }

        public void fixUpwardTree(AABBNode node)
        {
            // 거슬러 올라가면서 aabb 전부 다시 계산
            while (node != null)
            {
                AABBNode left = node.left;
                AABBNode right = node.right;
                node.aabb = left.aabb.union(right.aabb);
                node = node.parent;
            }
        }

        public void queryOverlaps(Collider collider,List<Collider> list)
        {
            _potentialNodeList.Clear();
            int readPosition = 0;
            _potentialNodeList.Add(_root);

            AABB testAABB = collider.getAABB();

            while(_potentialNodeList.Count - readPosition > 0)
            {
                AABBNode node = _potentialNodeList[readPosition];
                ++readPosition;

                // 가능한가??
                if (node == null)
                    continue;

                if(node.aabb.overlaps(testAABB))
                {
                    if(node.isLeaf() && node.collider != null)
					          {
                        if( node.collider != collider &&
                            node.collider.getGroupID() != collider.getGroupID())
						            {
                            list.Add(node.collider);
                        }
                    }
                    else
					          {
                        if( node.left != null)
						            {
                            _potentialNodeList.Add(node.left);
						            }
                        if( node.right != null)
						            {
                            _potentialNodeList.Add(node.right);
						            }
					          }
                }
            }
        }
    }
}
