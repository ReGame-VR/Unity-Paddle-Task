using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer : MonoBehaviour
{
    // For now, only support queues explicitly for Vector3s. 
    // TODO: make this generic. 
    private Queue<Vector3> _queue;

    private int _size;
    private int capacity = 0;

    public CircularBuffer(int size)
    {
        _queue = new Queue<Vector3>(size);
        _size = size;
    }

    public void Add(Vector3 o)
    {
        if (capacity < _size)
        {
            _queue.Enqueue(o);
            capacity++;
        }
        else
        {
            _queue.Dequeue();
            _queue.Enqueue(o);
        }
    }

    public Vector3[] GetArray()
    {
        return _queue.ToArray();
    }
}
