
namespace UniFan
{
    public abstract class ManagerBase
    {
        /// <summary>
        /// manager update优先级,数值小的优先执行
        /// </summary>
        public abstract int managerPriority { get; }

        public virtual void OnReconnect()
        {

        }

        public virtual void OnReset()
        {

        }

        public virtual void OnUpdate(float deltaTime)
        {

        }
    }

}
