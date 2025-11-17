using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Representa uma entidade no sistema ECS
    /// </summary>
    public class Entity
    {
        private readonly List<Component> _components = new List<Component>();
        
        public string Name { get; set; }
        public bool Active { get; set; } = true;

        public Entity(string name = "Entity")
        {
            Name = name;
        }

        /// <summary>
        /// Adiciona um componente à entidade
        /// </summary>
        public T AddComponent<T>(T component) where T : Component
        {
            component.Owner = this;
            _components.Add(component);
            return component;
        }

        /// <summary>
        /// Obtém um componente do tipo especificado
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            return _components.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Verifica se a entidade possui um componente do tipo especificado
        /// </summary>
        public bool HasComponent<T>() where T : Component
        {
            return _components.OfType<T>().Any();
        }

        /// <summary>
        /// Obtém todos os componentes da entidade
        /// </summary>
        public IEnumerable<Component> GetAllComponents()
        {
            return _components;
        }

        /// <summary>
        /// Remove um componente da entidade
        /// </summary>
        public void RemoveComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component != null)
            {
                _components.Remove(component);
            }
        }
    }
}
