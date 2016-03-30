﻿using System;
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
        public static int[] RequestCount = new int[11];

        public static List<TransportRessourceHandle>[] Offers = new List<TransportRessourceHandle>[11];
        public static List<TransportBuildingHandle>[] Needs = new List<TransportBuildingHandle>[11];

        public static Queue<Transaction> transactions = new Queue<Transaction>();
        static Mutex transactionQueueLock = new Mutex();
        static int transactionQueueLength = 0;

        public static Queue<GTransport> TransportQueue = new Queue<GTransport>();
        static Mutex transportLock = new Mutex();
        private static int transportQueueLength = 0;

        static bool running = true;

        public static void initialize()
        {
            for (int i = 0; i < OfferCount.Length; i++)
            {
                Offers[i] = new List<TransportRessourceHandle>();
                Needs[i] = new List<TransportBuildingHandle>();
            }
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
            RequestCount[(int)type]++;

            mutex.ReleaseMutex();
        }

        public static void resolveTransport()
        {
            transactionQueueLock.WaitOne();
            mutex.WaitOne();

            // TODO: OPTIONAL ?! MAKE THIS BETTER!

            for (int i = 0; i < OfferCount.Length; i++)
            {
                int min = Math.Min(OfferCount[i], RequestCount[i]);

                for (int j = 0; j < min; j++)
                {
                    transactions.Enqueue(new Transaction((ERessourceType)i, Offers[i][j].ID, Needs[i][j].ID, Offers[i][j].pos, Needs[i][j].pos));
                    transactionQueueLength++;
                }

                OfferCount[i] -= min;
                RequestCount[i] -= min;

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
                Thread.Sleep(10);
            }
        }

        public static void assignTransporters()
        {
            transactionQueueLock.WaitOne();
            transportLock.WaitOne();
            int min = Math.Min(transportQueueLength, transactionQueueLength);
            transactionQueueLock.ReleaseMutex();

            for (int i = 0; i < min; i++)
            {
                TransportQueue.Dequeue().setTransport(getFromQueue());
            }

            transportQueueLength -= min;
            transportQueueLength -= min;

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

            TransportQueue.Enqueue(gTransport);
            transportQueueLength++;

            transportLock.ReleaseMutex();
        }

        public static void stopTransportHandler()
        {
            running = false;
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