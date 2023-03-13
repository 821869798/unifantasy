
namespace UniFan
{
    public abstract class ManagerBase
    {
        /// <summary>
        /// manager update���ȼ�,��ֵС������ִ��
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
