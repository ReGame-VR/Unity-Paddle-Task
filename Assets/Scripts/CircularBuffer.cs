using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer<T> //: MonoBehaviour
{
    private Queue<T> _queue;

    private int _size;
    private int capacity = 0;

    public CircularBuffer(int size)
    {
        _queue = new Queue<T>(size);
        _size = size;
    }

    public int length()
    {
        return capacity;
    }

    public void Add(T o)
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

    public T[] GetArray()
    {
        return _queue.ToArray();
    }
}
