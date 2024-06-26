/* 2150998 张诚睿*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyLock
{
    public class Node
    {
        private volatile Thread thread;
        private const int BLOCK = 0;
        private const int RUNNING = 1;
        private int status;
        private bool isUpgrade;


        public Node(Thread _thread)
        {
            this.thread = _thread;
            status = BLOCK;
            isUpgrade = false;
        }
        public bool getIsUpgrade()
        {
            return isUpgrade;
        }
        public void makeUpgradeable()
        {
            isUpgrade = true;
        }

        public void awake()
        {
            if (status == BLOCK)
            {
                status = RUNNING;
            }
        }
        public Thread getThread()
        {
            return thread;
        }
        public int getStatus()
        {
            return status;
        }

    }
}
