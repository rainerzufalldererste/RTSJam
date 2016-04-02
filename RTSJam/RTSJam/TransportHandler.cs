using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading;

namespace RTSJam
{
    public static class TransportHandler
    {
        static Mutex mutex = new Mutex();

        public static int[] OfferCount = new int[11];
        public static int[] NeedCount = new int[11];

        public static List<TransportRessourceHandle>[] Offers = new List<TransportRessourceHandle>[11];
        public static List<TransportBuildingHandle>[] Needs = new List<TransportBuildingHandle>[11];

        public static Queue<Transaction> transactions = new Queue<Transaction>();
        static Mutex transactionQueueLock = new Mutex();

        public static List<GTransport> TransportList = new List<GTransport>();
        static Mutex transportLock = new Mutex();

        static bool running = true;
        static Thread thread;

        public static void initialize()
        {
            for (int i = 0; i < OfferCount.Length; i++)
            {
                Offers[i] = new List<TransportRessourceHandle>();
                Needs[i] = new List<TransportBuildingHandle>();
            }

            OfferCount = new int[11];
            NeedCount = new int[11];

            transactionQueueLock = new Mutex();
            transportLock = new Mutex();
            mutex = new Mutex();

            thread = new Thread(new ThreadStart(threadedResolve));
            thread.Start();

            running = true;
        }

        public static void placeOffer(ERessourceType type, TransportRessourceHandle handle)
        {
            mutex.WaitOne();

            Offers[(int)type].Add(handle);
            OfferCount[(int)type]++;

            mutex.ReleaseMutex();
        }

        public static void placeNeed(ERessourceType type, TransportBuildingHandle handle)
        {
            mutex.WaitOne();

            Needs[(int)type].Add(handle);
            NeedCount[(int)type]++;

            mutex.ReleaseMutex();
        }

        public static void resolveTransport()
        {
            transactionQueueLock.WaitOne();
            mutex.WaitOne();

            // TODO: OPTIONAL ?! MAKE THIS BETTER!

            for (int i = 0; i < OfferCount.Length; i++)
            {
                int min = Math.Min(OfferCount[i], NeedCount[i]);
                float mindist = float.MaxValue;
                float dist;
                int index = -1;

                for (int j = 0; j < min; j++)
                {
                    bool[] used = new bool[min];
                    index = -1;

                    for (int k = 0; k < min; k++)
                    {
                        if (!used[k])
                        {
                            dist = (Offers[i][j].pos - Needs[i][k].pos).Length();

                            if (dist < mindist)
                            {
                                if (index > 0)
                                    used[index] = false;

                                mindist = dist;
                                index = k;
                                used[k] = true;
                            }
                        }
                    }

                    transactions.Enqueue(new Transaction((ERessourceType)i, Offers[i][j].ID, Needs[i][index].ID, Offers[i][j].pos, Needs[i][index].pos));
                }

                OfferCount[i] -= min;
                NeedCount[i] -= min;

                Offers[i].RemoveRange(0, min);
                Needs [i].RemoveRange(0, min);
            }

            mutex.ReleaseMutex();
            transactionQueueLock.ReleaseMutex();
        }

        public static void threadedResolve()
        {
            while(running)
            {
                resolveTransport();
                assignTransporters();
                Thread.Sleep(3);
            }
        }

        public static void assignTransporters()
        {
            transactionQueueLock.WaitOne();
            transportLock.WaitOne();
            int min = Math.Min(transactions.Count, TransportList.Count);
            transactionQueueLock.ReleaseMutex();

            float mindist = float.MaxValue;
            float dist;
            int index = -1;

            for (int i = 0; i < min; i++)
            {
                bool[] used = new bool[min];
                index = -1;

                for (int j = 0; j < min; j++)
                {
                    if (!used[j])
                    {
                        dist = (TransportList[j].position - transactions.Peek().destiation).Length();

                        if(dist < mindist)
                        {
                            if (index > 0)
                                used[index] = false;

                            mindist = dist;
                            index = j;
                            used[j] = true;
                        }
                    }
                }

                TransportList[index].setTransport(getFromQueue());
                TransportList.RemoveAt(index);
            }

            transportLock.ReleaseMutex();
        }

        public static Transaction getFromQueue()
        {
            transactionQueueLock.WaitOne();

            Transaction t = transactions.Dequeue();
            
            transactionQueueLock.ReleaseMutex();

            return t;
        }

        public static void addFreeTransport(GTransport gTransport)
        {
            transportLock.WaitOne();

            TransportList.Add(gTransport);

            transportLock.ReleaseMutex();
        }

        public static void stopTransportHandler()
        {
            running = false;

            if(thread != null)
            {
                try
                {
                    thread.Abort();
                }
                catch(Exception) { }

                thread = null;
            }

            try
            {
                transactionQueueLock.ReleaseMutex();
            }
            catch (Exception) { }

            try
            {
                transportLock.ReleaseMutex();
            }
            catch (Exception) { }

            try
            {
                mutex.ReleaseMutex();
            }
            catch (Exception) { }

            transactionQueueLock = new Mutex();
            transportLock = new Mutex();
            mutex = new Mutex();
        }
    }
    
    public enum ERessourceType : byte
    {
        Stone = 0,
        Coal = 1,
        Iron = 2,
        IronBar = 3,
        Ice = 4,
        Water = 5,
        Food = 6,
        Gold = 7,
        GoldBar = 8,
        RawPurPur = 9,
        PurPur = 10
    }

    public struct Transaction
    {
        public ERessourceType type;
        public Ressource ressourceID;
        public GBuilding buildingID;
        public Vector2 origin;
        public Vector2 destiation;

        public Transaction(ERessourceType type, Ressource ressourceID, GBuilding buildingID, Vector2 pos, Vector2 dest) : this()
        {
            this.type = type;
            this.ressourceID = ressourceID;
            this.buildingID = buildingID;
            this.origin = pos;
            this.destiation = dest;
        }
    }

    public struct TransportBuildingHandle
    {
        public GBuilding ID;
        public Vector2 pos;

        public TransportBuildingHandle(GBuilding id, Vector2 p) : this()
        {
            ID = id;
            pos = p;
        }
    }
    public struct TransportRessourceHandle
    {
        public Ressource ID;
        public Vector2 pos;

        public TransportRessourceHandle(Ressource id, Vector2 p) : this()
        {
            ID = id;
            pos = p;
        }
    }
}
