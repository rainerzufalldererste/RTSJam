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

        public static int[] HOSTILEOfferCount = new int[11];
        public static int[] HOSTILENeedCount = new int[11];

        public static List<TransportRessourceHandle>[] Offers = new List<TransportRessourceHandle>[11];
        public static List<TransportBuildingHandle>[] Needs = new List<TransportBuildingHandle>[11];

        public static List<TransportRessourceHandle>[] HOSTILEOffers = new List<TransportRessourceHandle>[11];
        public static List<TransportBuildingHandle>[] HOSTILENeeds = new List<TransportBuildingHandle>[11];

        public static Queue<Transaction> transactions = new Queue<Transaction>();
        public static Queue<Transaction> HOSTILEtransactions = new Queue<Transaction>();
        static Mutex transactionQueueLock = new Mutex();

        public static List<GTransport> TransportList = new List<GTransport>();
        public static List<GTransport> HOSTILETransportList = new List<GTransport>();
        static Mutex transportLock = new Mutex();

        static bool running = true;
        static Thread thread;

        public static void initialize()
        {
            for (int i = 0; i < OfferCount.Length; i++)
            {
                Offers[i] = new List<TransportRessourceHandle>();
                Needs[i] = new List<TransportBuildingHandle>();

                HOSTILEOffers[i] = new List<TransportRessourceHandle>();
                HOSTILENeeds[i] = new List<TransportBuildingHandle>();
            }

            OfferCount = new int[11];
            NeedCount = new int[11];

            HOSTILEOfferCount = new int[11];
            HOSTILENeedCount = new int[11];

            transactions = new Queue<Transaction>();
            HOSTILEtransactions = new Queue<Transaction>();

            TransportList = new List<GTransport>();
            HOSTILETransportList = new List<GTransport>();

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

            if (handle.hostile)
            {
                HOSTILEOffers[(int)type].Add(handle);
                HOSTILEOfferCount[(int)type]++;
            }
            else
            {
                Offers[(int)type].Add(handle);
                OfferCount[(int)type]++;
            }

            mutex.ReleaseMutex();
        }

        public static void placeNeed(ERessourceType type, TransportBuildingHandle handle)
        {
            mutex.WaitOne();

            if (handle.hostile)
            {
                HOSTILENeeds[(int)type].Add(handle);
                HOSTILENeedCount[(int)type]++;
            }
            else
            {
                Needs[(int)type].Add(handle);
                NeedCount[(int)type]++;
            }

            mutex.ReleaseMutex();
        }

        public static void resolveTransport()
        {
            transactionQueueLock.WaitOne();
            mutex.WaitOne();

            for (int i = 0; i < OfferCount.Length; i++)
            {
                int min = Math.Min(OfferCount[i], NeedCount[i]);
                float mindist = float.MaxValue;
                float dist;
                int index = -1;

                for (int j = 0; j < min; j++)
                {
                    bool[] used = new bool[Offers[i].Count];
                    index = -1;
                    mindist = float.MaxValue;

                    for (int k = 0; k < Offers[i].Count; k++)
                    {
                        if (!used[k])
                        {
                            dist = (Offers[i][k].pos - Needs[i][j].pos).Length();

                            if (dist < mindist)
                            {
                                if (index >= 0)
                                    used[index] = false;

                                mindist = dist;
                                index = k;
                                used[k] = true;
                            }
                        }
                    }

                    transactions.Enqueue(new Transaction((ERessourceType)i, Offers[i][index].ID, Needs[i][j].ID, Offers[i][index].pos, Needs[i][j].pos, false));
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

                resolveHOSTILETransport();
                assignHOSTILETransporters();
                Thread.Sleep(3);
            }
        }

        private static void assignHOSTILETransporters()
        {
            transactionQueueLock.WaitOne();
            transportLock.WaitOne();
            int min = Math.Min(HOSTILEtransactions.Count, HOSTILETransportList.Count);
            transactionQueueLock.ReleaseMutex();

            float mindist = float.MaxValue;
            float dist;
            int index = -1;

            for (int i = 0; i < min; i++)
            {
                bool[] used = new bool[HOSTILETransportList.Count];
                index = -1;
                mindist = float.MaxValue;

                for (int j = 0; j < HOSTILETransportList.Count; j++)
                {
                    if (!used[j])
                    {
                        dist = (HOSTILETransportList[j].position - HOSTILEtransactions.Peek().destiation).Length();

                        if (dist < mindist)
                        {
                            if (index >= 0)
                                used[index] = false;

                            mindist = dist;
                            index = j;
                            used[j] = true;
                        }
                    }
                }

                if (index >= 0)
                {
                    HOSTILETransportList[index].setTransport(getFromHOSTILEQueue());
                    HOSTILETransportList.RemoveAt(index);
                }
            }

            transportLock.ReleaseMutex();
        }

        private static void resolveHOSTILETransport()
        {
            transactionQueueLock.WaitOne();
            mutex.WaitOne();

            for (int i = 0; i < HOSTILEOfferCount.Length; i++)
            {
                int min = Math.Min(HOSTILEOfferCount[i], HOSTILENeedCount[i]);
                float mindist = float.MaxValue;
                float dist;
                int index = -1;

                for (int j = 0; j < min; j++)
                {
                    bool[] used = new bool[HOSTILEOffers[i].Count];
                    index = -1;
                    mindist = float.MaxValue;

                    for (int k = 0; k < HOSTILEOffers[i].Count; k++)
                    {
                        if (!used[k])
                        {
                            dist = (HOSTILEOffers[i][k].pos - HOSTILENeeds[i][j].pos).Length();

                            if (dist < mindist)
                            {
                                if (index >= 0)
                                    used[index] = false;

                                mindist = dist;
                                index = k;
                                used[k] = true;
                            }
                        }
                    }

                    HOSTILEtransactions.Enqueue(new Transaction((ERessourceType)i, HOSTILEOffers[i][index].ID, HOSTILENeeds[i][j].ID, HOSTILEOffers[i][index].pos, HOSTILENeeds[i][j].pos, true));
                }

                HOSTILEOfferCount[i] -= min;
                HOSTILENeedCount[i] -= min;

                HOSTILEOffers[i].RemoveRange(0, min);
                HOSTILENeeds[i].RemoveRange(0, min);
            }

            mutex.ReleaseMutex();
            transactionQueueLock.ReleaseMutex();
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
                bool[] used = new bool[TransportList.Count];
                index = -1;
                mindist = float.MaxValue;

                for (int j = 0; j < TransportList.Count; j++)
                {
                    if (!used[j])
                    {
                        dist = (TransportList[j].position - transactions.Peek().destiation).Length();

                        if(dist < mindist)
                        {
                            if (index >= 0)
                                used[index] = false;

                            mindist = dist;
                            index = j;
                            used[j] = true;
                        }
                    }
                }

                if (index >= 0)
                {
                    TransportList[index].setTransport(getFromQueue());
                    TransportList.RemoveAt(index);
                }
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
        public static Transaction getFromHOSTILEQueue()
        {
            transactionQueueLock.WaitOne();

            Transaction t = HOSTILEtransactions.Dequeue();

            transactionQueueLock.ReleaseMutex();

            return t;
        }

        public static void addFreeTransport(GTransport gTransport)
        {
            transportLock.WaitOne();

            if (gTransport.hostile)
            {
                HOSTILETransportList.Add(gTransport);
            }
            else
            {
                TransportList.Add(gTransport);
            }

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
        IronIngot = 3,
        Ice = 4,
        Water = 5,
        Food = 6,
        Gold = 7,
        GoldIngot = 8,
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
        public bool hostile;

        public Transaction(ERessourceType type, Ressource ressourceID, GBuilding buildingID, Vector2 pos, Vector2 dest, bool hostile) : this()
        {
            this.type = type;
            this.ressourceID = ressourceID;
            this.buildingID = buildingID;
            this.origin = pos;
            this.destiation = dest;
            this.hostile = hostile;
        }
    }

    public struct TransportBuildingHandle
    {
        public GBuilding ID;
        public Vector2 pos;
        public bool hostile;

        public TransportBuildingHandle(GBuilding id, Vector2 p, bool hostile) : this()
        {
            ID = id;
            pos = p;
            this.hostile = hostile;
        }
    }
    public struct TransportRessourceHandle
    {
        public Ressource ID;
        public Vector2 pos;
        public bool hostile;

        public TransportRessourceHandle(Ressource id, Vector2 p, bool hostile) : this()
        {
            ID = id;
            pos = p;
            this.hostile = hostile;
        }
    }
}
