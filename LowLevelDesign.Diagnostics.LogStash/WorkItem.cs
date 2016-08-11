using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LowLevelDesign.Diagnostics.LogStash
{
    /// <summary>
    /// Base class for all types of background tasks to be done
    /// by the worker thread.
    /// </summary>
    public abstract class WorkItem
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Queue<WorkItem> queue = new Queue<WorkItem>();
        private static Semaphore maxQueueSemaphore = new Semaphore(MaxQueueLength, MaxQueueLength);
        private static object workItemLockObject = new object();
        private static WorkItem currentWorkItem;
        private static Thread worker;

        /// <summary>
        /// Enqueues the given work item to be executed by the worker
        /// thread at some time in the future.
        /// </summary>
        public void Enqueue()
        {
            Init();
            if (maxQueueSemaphore.WaitOne(1000))
            {
                lock (queue)
                {
                    queue.Enqueue(this);
                    Monitor.Pulse(queue);
                }
            }
            else
            {
                throw new TimeoutException("Timed-out enqueueing a WorkItem. Queue size = " + QueueCount);
            }
        }

        /// <summary>
        /// Maximum size of the task waiting queue.
        /// </summary>
        public static int MaxQueueLength
        {
            get { return 30; }
        }

        /// <summary>
        /// Number of tasks currently enqueued.
        /// </summary>
        public static int QueueCount
        {
            get { return queue.Count; }
        }

        public static void Abort()
        {
            if (worker != null && worker.IsAlive) {
                worker.Abort();
            }
        }


        private static WorkItem Dequeue()
        {
            lock (queue)
            {
                for (;;)
                {
                    if (queue.Count > 0)
                    {
                        WorkItem workItem = queue.Dequeue();
                        maxQueueSemaphore.Release();
                        return workItem;
                    }
                    Monitor.Wait(queue);
                }
            }
        }

        private static void Init()
        {
            lock (workItemLockObject)
            {
                if (worker == null || !worker.IsAlive)
                {
                    worker = new Thread(new ThreadStart(TaskLoop));
                    worker.Start();
                }
            }
        }

        /// <summary>
        /// Actual implementation of the work item task.
        /// </summary>
        protected abstract void DoWork();

        private static void TaskLoop()
        {
            try
            {
                for (;;)
                {
                    WorkItem workItem = Dequeue();
                    workItem.DoWork();
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                // FIXME we should probable swallow this exception not to terminate the app
                logger.Error(e, "Error in the worker thread");
            }
        }
    }
}
