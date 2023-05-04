
namespace UniFan
{
    public interface IReusableClass
    {
        //最大保留数量
        uint MaxStore { get; }
        //重置
        void OnReset();
    }

}

