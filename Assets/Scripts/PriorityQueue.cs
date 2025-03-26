using System;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    private class Node
    {
        Vector2Int _position;
        float _priority;
        public Node(Vector2Int position, float priority)
        {
            _position = position;
            _priority = priority;
        }
        public Vector2Int Position
        {
            get { return _position; }
        }
        public float Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }
    }
    private Node[] nodes;
    private int size;
    private int capacity;
    private void Swap(int a, int b)
    {
        if (a == b) return;
        Node temp = nodes[a];
        nodes[a] = nodes[b];
        nodes[b] = temp;
    }
    private void RaiseCapacity()
    {
        capacity *= 2;
        Array.Resize(ref nodes, capacity);
    }
    private void LowerCapacity()
    {
        capacity /= 2;
        Array.Resize(ref nodes, capacity);
    }
    public PriorityQueue()
    {
        capacity = 1;
        nodes = new Node[capacity];
        size = 0;
    }
    public int Count
    {
        get { return size; }
    }
    public void Clear()
    {
        capacity = 1;
        nodes = new Node[capacity];
        size = 0;
    }
    public void Enqueue(Vector2Int position, float priority)
    {
        size++;
        if (size > capacity) RaiseCapacity();
        Node n = new Node(position, priority);
        int index = size - 1;
        nodes[index] = n;
        HeapUp(index);

    }
    private void HeapUp(int index)
    {
        int parent = (index - 1) / 2;
        while (index > 0 && nodes[parent].Priority > nodes[index].Priority)
        {
            Swap(index, parent);
            index = parent;
            parent = (index - 1) / 2;
        }
    }
    public Vector2Int Dequeue()
    {
        if (size == 0) return new Vector2Int(-1, -1);
        Vector2Int ret = nodes[0].Position;
        size--;
        if (size < (capacity / 2) && size > 64) LowerCapacity();
        int index = 0;
        Swap(index, size);
        HeapDown(index);
        return ret;
    }
    private void HeapDown(int index)
    {
        int childA = (2 * index) + 1;
        int childB = (2 * index) + 2;
        while (childA < size && childB < size)
        {
            int smallestChild = (nodes[childA].Priority < nodes[childB].Priority) ? childA : childB;
            if (nodes[index].Priority <= nodes[smallestChild].Priority) break;
            Swap(index, smallestChild);
            index = smallestChild;
            childA = (2 * index) + 1;
            childB = (2 * index) + 2;
        }
    }
    public Vector2Int Peek()
    {
        return nodes[0].Position;
    }
    public void Remove(Vector2Int target)
    {
        int index = 0;
        while (index < size && !target.Equals(nodes[index].Position)) index++;
        if (index == size) return;
        size--;
        if (size < (capacity / 2) && size > 1) LowerCapacity();
        Swap(index, size);
        HeapDown(index);
    }
    public void AdjustPriority(Vector2Int target, float priority)
    {
        int index = 0;
        while (index < size && !target.Equals(nodes[index].Position)) index++;
        if (index == size) return;
        if (priority < nodes[index].Priority)
        {
            nodes[index].Priority = priority;
            HeapUp(index);
        }
        else if (priority > nodes[index].Priority)
        {
            nodes[index].Priority = priority;
            HeapDown(index);
        }
    }
}