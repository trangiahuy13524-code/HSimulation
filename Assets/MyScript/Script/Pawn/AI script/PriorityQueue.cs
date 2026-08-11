using System.Collections.Generic;

public class PriorityQueue<T>
{
    struct Node
    {
        public T Item;
        public int Priority;

        public Node(T item, int priority)
        {
            Item = item;
            Priority = priority;
        }
    }

    readonly List<Node> heap = new();

    public int Count => heap.Count;

    // ======================================================
    // ADD
    // ======================================================
    public void Enqueue(T item, int priority)
    {
        heap.Add(new Node(item, priority));
        HeapifyUp(heap.Count - 1);
    }

    // ======================================================
    // REMOVE BEST
    // ======================================================
    public T Dequeue()
    {
        int lastIndex = heap.Count - 1;

        Node root = heap[0];
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
            HeapifyDown(0);

        return root.Item;
    }

    // ======================================================
    // CLEAR (NO GC)
    // ======================================================
    public void Clear()
    {
        heap.Clear();
    }

    // ======================================================
    // HEAP OPERATIONS
    // ======================================================
    void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (heap[index].Priority >= heap[parent].Priority)
                break;

            Swap(index, parent);
            index = parent;
        }
    }

    void HeapifyDown(int index)
    {
        int last = heap.Count - 1;

        while (true)
        {
            int left = index * 2 + 1;
            int right = left + 1;
            int smallest = index;

            if (left <= last &&
                heap[left].Priority < heap[smallest].Priority)
                smallest = left;

            if (right <= last &&
                heap[right].Priority < heap[smallest].Priority)
                smallest = right;

            if (smallest == index)
                return;

            Swap(index, smallest);
            index = smallest;
        }
    }

    void Swap(int a, int b)
    {
        Node temp = heap[a];
        heap[a] = heap[b];
        heap[b] = temp;
    }
}