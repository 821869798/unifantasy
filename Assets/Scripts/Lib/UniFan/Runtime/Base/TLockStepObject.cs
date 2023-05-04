
namespace UniFan
{
    //帧同步对象基类
    public abstract class TLockStepObject : TObject
    {
        /// <summary>
        /// 逻辑帧更新
        /// </summary>
        public virtual void OnUpdateLogic() { }

        /// <summary>
        /// 渲染帧更新（用来更新位置等跟逻辑无关的东西）
        /// </summary>
        /// <param name="deltaTime">deltaTime</param>
        /// <param name="interpolation">插值(0-1)</param>
        public virtual void OnUpdateRender(float deltaTime, float interpolation) { }

        //禁用该方法
        public sealed override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }
    }

}
