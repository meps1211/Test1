using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using Ozeki.VoIP;

namespace DialerNS
{

    class myQueue : BlockingQueue<DialerEvent>
    {
        public override bool TryDequeue(out DialerEvent dialerEvent, int timeOut)
        {
            bool isTimeOut = base.TryDequeue(out dialerEvent, timeOut);
            if (isTimeOut)
            {
                dialerEvent = new DialerEvent(eEventType.TimeOut);
            }
            Console.WriteLine("Dequeue: {0}", dialerEvent.EventType);

            return isTimeOut;
           
        }
    }

    public class BlockingQueue<T> where T : class
    {
        private bool _closing;
        private readonly Queue<T> _queue = new Queue<T>();

        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        public BlockingQueue()
        {
            lock (_queue)
            {
                _closing = false;
                Monitor.PulseAll(_queue);
            }
        }

        public bool Enqueue(T item)
        {
            lock (_queue)
            {
                if (_closing || item == null)
                {
                    return false;
                }

                _queue.Enqueue(item);

                if (_queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(_queue);
                }

                return true;
            }
        }


        public void Close()
        {
            lock (_queue)
            {
                if (!_closing)
                {
                    _closing = true;
                    _queue.Clear();
                    Monitor.PulseAll(_queue);
                }
            }
        }


        virtual public bool TryDequeue(out T value, int timeout = Timeout.Infinite)
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                {
                    if (_closing || (timeout < Timeout.Infinite) || !Monitor.Wait(_queue, timeout))
                    {
                        value = default(T);
                        return true;
                    }
                }

                value = _queue.Dequeue();              
                return false;
            }
        }

        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
                Monitor.Pulse(_queue);
            }
        }
    }
}