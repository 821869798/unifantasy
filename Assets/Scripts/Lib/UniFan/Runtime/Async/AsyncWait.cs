using System.Collections;

namespace UniFan
{
    public class AsyncWait : IEnumerator
    {
        public object Current => Result;

        public bool IsDone { get; set; }

        public object Result { get; set; }

        public bool MoveNext()
        {
            return !IsDone;
        }

        public void Reset()
        {
            IsDone = false;
            Result = null;
        }
    }
}
