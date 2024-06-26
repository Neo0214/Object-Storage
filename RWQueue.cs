/* 2150998 张诚睿*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLock
{
    public class RWQueue
    {
        public const int READ = 0;
        public const int WRITE = 1;
        private const int maxCount = 3;

        private int count;
        private int finished;
        private int type;
        private volatile RWQueue next;
        private volatile Node[] nodes;
        public RWQueue(int type)
        {
            next = null;
            nodes = new Node[]
            {
                new Node(null),
            new Node(null),
            new Node(null)

            };
            count = 0;
            this.type = type;
            finished = 0;
        }

        public void setNext(RWQueue next)
        {
            this.next = next;
        }
        public RWQueue getNext()
        {
            return next;
        }

        public int getCount()
        {
            return count;
        }
        public int addNode(Thread thread, bool upgradeable = false)
        {

            nodes[count++] = new Node(thread);
            if (upgradeable)
            {
                nodes[count - 1].makeUpgradeable();
            }
            //Console.WriteLine("type: " + type + " count: " + count);
            return count - 1;
        }

        public bool isFull()
        {
            return count == maxCount;
        }

        public int getType()
        {
            return type;
        }

        public void awakeAll()
        {
            for (int i = 0; i < count; i++)
            {
                nodes[i].awake();
            }
        }

        public void finishOne()
        {
            finished++;
        }

        public bool allFinished()
        {
            //Console.WriteLine("finished : " + finished + " count: " + count);
            return finished == count;
        }

        public Node getFirst()
        {
            return nodes[0];
        }

        public Node getNode(int i)
        {
            return nodes[i];
        }

        public bool noUpgrade()
        {
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].getIsUpgrade())
                {
                    return false;
                }
            }
            return true;
        }

        public Thread getUpgrade()
        {
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].getIsUpgrade())
                {
                    return nodes[i].getThread();
                }
            }
            return null;
        }
    }
}
