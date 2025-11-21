using System;
using System.Collections.Generic;

namespace CubeSurvivor.Core.Registry
{
    /// <summary>
    /// Implementação base de registry genérico
    /// Princípio: Single Responsibility Principle (SRP) - Responsável apenas por gerenciar registros
    /// Princípio: Open/Closed Principle (OCP) - Pode ser estendido sem modificação
    /// </summary>
    public class Registry<TKey, TValue> : IRegistry<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
        private readonly object _lock = new object();

        public virtual void Register(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (_items.ContainsKey(key))
                    throw new InvalidOperationException($"Item with key '{key}' is already registered.");

                _items[key] = value;
            }
        }

        public virtual TValue Get(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (!_items.TryGetValue(key, out var value))
                    throw new KeyNotFoundException($"No item registered with key '{key}'.");

                return value;
            }
        }

        public bool Contains(TKey key)
        {
            if (key == null)
                return false;

            lock (_lock)
            {
                return _items.ContainsKey(key);
            }
        }

        public IEnumerable<TKey> GetAllKeys()
        {
            lock (_lock)
            {
                return new List<TKey>(_items.Keys);
            }
        }

        public IEnumerable<TValue> GetAllValues()
        {
            lock (_lock)
            {
                return new List<TValue>(_items.Values);
            }
        }

        public bool Unregister(TKey key)
        {
            if (key == null)
                return false;

            lock (_lock)
            {
                return _items.Remove(key);
            }
        }
    }
}
