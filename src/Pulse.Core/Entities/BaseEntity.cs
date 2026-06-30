/// <summary>
/// A generic base entity class with an ID of type T.
/// </summary>
/// <typeparam name="T">The type of the ID (e.g., int, string).</typeparam>

namespace Pulse.Core.Entities
{
    public abstract class BaseEntity<T>
    {
        /// <summary>
        /// The unique identifier of the entity.
        /// </summary>
        public T Id { get; set; }
    }
}
