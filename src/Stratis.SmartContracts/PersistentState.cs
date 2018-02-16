﻿using System;
using System.Text;
using NBitcoin;
using Stratis.SmartContracts.State;

namespace Stratis.SmartContracts
{
    public class PersistentState
    {
        internal static IContractStateRepository StateDb { get; private set; }

        private uint counter;
        public uint160 ContractAddress { get; }
        private static readonly PersistentStateSerializer serializer = new PersistentStateSerializer();

        /// <summary>
        /// Instantiate a new PersistentState instance. Each PersistentState object represents
        /// a slice of state for a particular contract address.
        /// </summary>
        /// <param name="stateDb"></param>
        /// <param name="contractAddress"></param>
        public PersistentState(IContractStateRepository stateDb, uint160 contractAddress)
        {
            StateDb = stateDb;
            this.ContractAddress = contractAddress;
            this.counter = 0;
        }

        public T GetObject<T>(object key)
        {
            byte[] keyBytes = serializer.Serialize(key);
            byte[] bytes = StateDb.GetStorageValue(this.ContractAddress, keyBytes);

            if (bytes == null)
                return default(T);

            return serializer.Deserialize<T>(bytes);
        }

        public void SetObject<T>(object key, T obj)
        {
            byte[] keyBytes = serializer.Serialize(key);
            StateDb.SetStorageValue(this.ContractAddress, keyBytes, serializer.Serialize(obj));
        }

        public SmartContractMapping<K, V> GetMapping<K, V>()
        {
            return new SmartContractMapping<K, V>(this, this.counter++);
        }

        public SmartContractList<T> GetList<T>()
        {
            return new SmartContractList<T>(this, this.counter++);
        }
    }

    /// <summary>
    /// The end goal for this class is to take in any object and serialize it to bytes, or vice versa.
    /// Will likely need to be highly complex in the future but right now we just fall back to JSON worst case.
    /// This idea may be ridiculous so we can always have custom methods that have to be called on PersistentState in the future.
    /// </summary>
    public class PersistentStateSerializer
    {
        public byte[] Serialize(object o)
        {
            if (o is byte[])
                return (byte[])o;

            if (o is byte)
                return new byte[] { (byte)o };

            if (o is char)
                return new byte[] { Convert.ToByte(((char)o)) };

            if (o is Address)
                return ((Address)o).ToUint160().ToBytes();

            if (o is bool)
                return (BitConverter.GetBytes((bool)o));

            if (o is int)
                return BitConverter.GetBytes((int)o);

            if (o is long)
                return BitConverter.GetBytes((long)o);

            if (o is uint)
                return BitConverter.GetBytes((uint)o);

            if (o is ulong)
                return BitConverter.GetBytes((ulong)o);

            if (o is sbyte)
                return BitConverter.GetBytes((sbyte)o);

            if (o is string)
                return Encoding.UTF8.GetBytes((string)o);

            throw new Exception(string.Format("{0} is not supported.", o.GetType().Name));
        }

        public T Deserialize<T>(byte[] stream)
        {
            if (stream == null || stream.Length == 0)
                return default(T);

            if (typeof(T) == typeof(byte[]))
                return (T)(object)stream;

            if (typeof(T) == typeof(byte))
                return (T)(object)stream[0];

            if (typeof(T) == typeof(char))
                return (T)(object)Convert.ToChar(stream[0]);

            if (typeof(T) == typeof(Address))
                return (T)(object)new Address(new uint160(stream));

            if (typeof(T) == typeof(bool))
                return (T)(object)(Convert.ToBoolean(stream[0]));

            if (typeof(T) == typeof(int))
                return (T)(object)(BitConverter.ToInt32(stream, 0));

            if (typeof(T) == typeof(long))
                return (T)(object)(BitConverter.ToInt64(stream, 0));

            if (typeof(T) == typeof(sbyte))
                return (T)(object)(Convert.ToSByte(stream[0]));

            if (typeof(T) == typeof(string))
                return (T)(object)(Encoding.UTF8.GetString(stream));

            if (typeof(T) == typeof(uint))
                return (T)(object)(BitConverter.ToUInt32(stream, 0));

            if (typeof(T) == typeof(ulong))
                return (T)(object)(BitConverter.ToUInt64(stream, 0));

            throw new Exception(string.Format("{0} is not supported.", typeof(T).Name));
        }
    }
}