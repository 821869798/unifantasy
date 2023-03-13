using UniFan;

namespace UniFan.Res
{
    public interface IResLoader : IReusableClass
    {
        void ReleaseAllRes();

        void Put2Pool();
    }

}
