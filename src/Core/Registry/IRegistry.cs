using System;
using System.Collections.Generic;

namespace CubeSurvivor.Core.Registry
{
    /// <summary>
    /// Interface base para registries genéricos
    /// Princípio: Interface Segregation Principle (ISP) - Define apenas operações essenciais
    /// </summary>
    public interface IRegistry<TKey, TValue>
    {
        /// <summary>
        /// Registra um item no registry
        /// </summary>
        void Register(TKey key, TValue value);

        /// <summary>
        /// Obtém um item registrado
        /// </summary>
        TValue Get(TKey key);

        /// <summary>
        /// Verifica se uma chave está registrada
        /// </summary>
        bool Contains(TKey key);

        /// <summary>
        /// Obtém todas as chaves registradas
        /// </summary>
        IEnumerable<TKey> GetAllKeys();

        /// <summary>
        /// Obtém todos os valores registrados
        /// </summary>
        IEnumerable<TValue> GetAllValues();

        /// <summary>
        /// Remove um item do registry
        /// </summary>
        bool Unregister(TKey key);
    }
}
