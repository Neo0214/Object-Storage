/* 2150998 张诚睿*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLock
{
    public class MyReaderWriterLockSlim
    {
        private volatile object _lock;
        private volatile RWQueue queue;
        private volatile RWQueue head;
        private volatile RWQueue tail;
        private volatile RWQueue cur;
        private volatile int reentrans;

        public MyReaderWriterLockSlim()
        {
            queue = new RWQueue(RWQueue.READ); // 初始队列节点，无意义的头节点
            head = queue;
            cur = null;
            tail = queue;
            _lock = new object();
            reentrans = 0;
        }
        private int isExec(Thread thread)
        {
            // 判断是否当前线程已在执行
            int total = cur.getCount();
            for (int i = 0; i < total; i++)
            {
                if (cur.getNode(i).getThread() == thread)
                {
                    return i;
                }
            }
            return -1;
        }

        public void EnterReadLock()
        {
            // 首先进行队列安排
            RWQueue inQueue = null;
            Monitor.Enter(_lock);
            try
            {
                int index = -1;
                //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " is judge: " + (cur == null ? "-2" : isExec(Thread.CurrentThread)));
                if (cur != null && -1 != (index = isExec(Thread.CurrentThread)))
                {
                    // 重入
                    reentrans++;
                    return;
                }
                if (tail == head)
                { // 队列为空
                  // 创建新队列
                  //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " in if");
                    RWQueue newQueue = new RWQueue(RWQueue.READ);
                    newQueue.addNode(Thread.CurrentThread);
                    tail.setNext(newQueue);
                    tail = newQueue;
                    inQueue = newQueue;
                    cur = inQueue;
                    reentrans++; // 因为它立刻就会获得锁，所以认为进入一次
                }
                else
                { // 队列有内容
                    //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " in else");
                    RWQueue curTail = tail;
                    if (tail.getType() == RWQueue.WRITE || tail.isFull())
                    { // 已满或是写队列
                      // 此时需要新建下一队列
                        RWQueue newQueue = new RWQueue(RWQueue.READ);
                        newQueue.addNode(Thread.CurrentThread);
                        tail.setNext(newQueue);
                        tail = newQueue;
                        inQueue = newQueue;
                    }
                    else
                    { // 未满，可以加入
                        curTail.addNode(Thread.CurrentThread);
                        inQueue = curTail;
                        if (-1 != (index = isExec(Thread.CurrentThread)))
                        { // 如果加入的队列是否是正在执行的队列，重入一次
                            reentrans++;
                        }
                    }

                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            // 等待
            while (cur != inQueue)
            {
                // 阻塞
            }
            //cur.awakeAll();

        }
        public void ExitReadLock()
        {
            Monitor.Enter(_lock);
            try
            {
                //cur.finishOne();
                reentrans--;
                if (reentrans != 0)
                {
                    // 还有重入的
                    return;
                }
                else /*(cur.allFinished())*/
                {

                    // 所有线程都完成了，可以释放队列
                    RWQueue next = cur.getNext();
                    if (next != null)
                    { // 有下一个节点
                        head.setNext(next);
                        cur = next;
                        if (next.getType() == RWQueue.READ)
                        {
                            // 如果下一节点是读节点，重入量设为读节点数量
                            reentrans = next.getCount();
                        }
                        else
                        {
                            // 如果下一节点是写节点，重入量设为1
                            reentrans = 1;

                        }
                    }
                    else
                    { // 没有下一个节点了
                        tail = head;
                        cur = null;
                        reentrans = 0;
                    }

                }
            }
            finally
            {

                Monitor.Exit(_lock);
            }
        }

        public void EnterWriteLock()
        {
            RWQueue inQueue = null;
            Monitor.Enter(_lock);
            try
            {
                // 对于写节点来说，如果在可升级情况下准备进入写模式，直接等待其它队列中的读节点完成
                if (cur != null && cur.getType() == RWQueue.READ && cur.getUpgrade() == Thread.CurrentThread)
                {
                    //Console.WriteLine("in upgrade thread: " + Thread.CurrentThread.ManagedThreadId + " in enter");
                    reentrans++;
                    inQueue = cur;
                }
                else
                {
                    // 对于写节点来说，在未升级情况下无论如何都要新加到末尾
                    if (cur != null && cur.getType() == RWQueue.WRITE && cur.getFirst().getThread() == Thread.CurrentThread)
                    {
                        // 重入
                        //Console.WriteLine("Write " + Thread.CurrentThread.ManagedThreadId + " rein");
                        reentrans++;
                        return;
                    }
                    if (head == tail)
                    {
                        // 空的，直接放入
                        //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " in if");
                        RWQueue newQueue = new RWQueue(RWQueue.WRITE);
                        newQueue.addNode(Thread.CurrentThread);
                        tail.setNext(newQueue);
                        tail = newQueue;
                        inQueue = newQueue;
                        cur = inQueue;
                        reentrans++; // 因为它立刻就会获得锁，所以认为进入一次
                    }
                    else
                    {
                        // 此时需要新建下一队列
                        //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " in else");
                        RWQueue newQueue = new RWQueue(RWQueue.WRITE);
                        newQueue.addNode(Thread.CurrentThread);
                        tail.setNext(newQueue);
                        tail = newQueue;
                        inQueue = newQueue;
                        // 肯定不会立刻执行，所以不用reentrans
                    }

                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            while (cur != inQueue || (cur.getType() == RWQueue.READ && reentrans != 2))
            {
                // 阻塞
            }
            //cur.awakeAll();
        }

        public void ExitWriteLock()
        {
            Monitor.Enter(_lock);
            try
            {
                reentrans--;
                if (reentrans != 0)
                {
                    return;
                }
                // 完成了，可以释放队列
                RWQueue next = cur.getNext();
                if (next != null)
                { // 有下一个节点
                    //Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " go to next");
                    head.setNext(next);
                    if (next.getType() == RWQueue.WRITE)
                    {
                        // 如果下一节点是写节点，重入量设为1
                        reentrans = 1;
                    }
                    else
                    {
                        // 如果下一节点是读节点，重入量设为读节点数量
                        reentrans = next.getCount();
                    }
                    cur = next;

                }
                else
                { // 没有下一个节点了
                    //Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " no next");
                    tail = head;
                    cur = null;
                    reentrans = 0; // 因为下一次获得锁的节点一定会立刻执行，所以重入次数清零
                }
            }
            finally
            {
                //Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " complete");
                Monitor.Exit(_lock);
            }
        }

        public void EnterUpgradeableReadLock()
        {
            // 正常流程和读锁类似
            // 首先进行队列安排
            RWQueue inQueue = null;
            Monitor.Enter(_lock);
            try
            {
                int index = -1;
                //Console.WriteLine("in thread: " + Thread.CurrentThread.ManagedThreadId + " is judge: " + (cur == null ? "-2" : isExec(Thread.CurrentThread)));
                if (tail == head)
                { // 队列为空
                  // 创建新队列
                  //Console.WriteLine("in upgrade thread: " + Thread.CurrentThread.ManagedThreadId + " in if");
                    RWQueue newQueue = new RWQueue(RWQueue.READ);
                    newQueue.addNode(Thread.CurrentThread, true); // 此处设置改node为可升级的
                    tail.setNext(newQueue);
                    tail = newQueue;
                    inQueue = newQueue;
                    cur = inQueue;
                    reentrans++; // 因为它立刻就会获得锁，所以认为进入一次
                }
                else
                { // 队列有内容
                    //Console.WriteLine("in upgrade thread: " + Thread.CurrentThread.ManagedThreadId + " in else");
                    RWQueue curTail = tail;
                    if (tail.getType() == RWQueue.WRITE || tail.isFull() || tail.noUpgrade() == false)
                    { // 已满或是写队列或是已有升级节点
                      // 此时需要新建下一队列
                      //Console.WriteLine("in upgrade thread: " + Thread.CurrentThread.ManagedThreadId + " in else new");
                        RWQueue newQueue = new RWQueue(RWQueue.READ);
                        newQueue.addNode(Thread.CurrentThread, true);
                        tail.setNext(newQueue);
                        tail = newQueue;
                        inQueue = newQueue;
                    }
                    else
                    { // 未满，可以加入
                        //Console.WriteLine("in upgrade thread: " + Thread.CurrentThread.ManagedThreadId + " in else add");
                        curTail.addNode(Thread.CurrentThread, true);
                        inQueue = curTail;
                        if (-1 != (index = isExec(Thread.CurrentThread)))
                        { // 如果加入的队列是否是正在执行的队列，重入一次
                            reentrans++;
                        }
                    }

                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            // 等待
            while (cur != inQueue)
            {
                // 阻塞
            }
        }

        public void ExitUpgradeableReadLock()
        {
            // 和读锁一样
            Monitor.Enter(_lock);
            try
            {
                //cur.finishOne();
                reentrans--;
                if (reentrans != 0)
                {
                    // 还有重入的
                    return;
                }
                else /*(cur.allFinished())*/
                {

                    // 所有线程都完成了，可以释放队列
                    RWQueue next = cur.getNext();
                    if (next != null)
                    { // 有下一个节点
                        head.setNext(next);
                        cur = next;
                        if (next.getType() == RWQueue.READ)
                        {
                            // 如果下一节点是读节点，重入量设为读节点数量
                            reentrans = next.getCount();
                        }
                        else
                        {
                            // 如果下一节点是写节点，重入量设为1
                            reentrans = 1;

                        }
                    }
                    else
                    { // 没有下一个节点了
                        tail = head;
                        cur = null;
                        reentrans = 0;
                    }

                }
            }
            finally
            {

                Monitor.Exit(_lock);
            }
        }
    }
}