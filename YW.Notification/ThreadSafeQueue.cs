﻿using System;
using System.Collections.Generic;

namespace YW.Notification
{
	public class ThreadSafeQueue<T>
	{

		public ThreadSafeQueue()
		{
			queue = new Queue<T>();
			lockObj = new object();
		}

		Queue<T> queue;
		object lockObj;

		public T Dequeue()
		{
			lock (lockObj)
			{
				return queue.Dequeue();
			}
		}

		public void Enqueue(T item)
		{
			lock (lockObj)
			{
				queue.Enqueue(item);
			}
		}

		public int Count
		{
			get { return queue.Count; }
		}

	}
}
