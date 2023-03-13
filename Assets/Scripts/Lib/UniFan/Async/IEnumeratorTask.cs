using System.Collections;
using System.Collections.Generic;

namespace UniFan
{
    public interface IEnumeratorTask
    {
        IEnumerator DoIEnumeratorTask(System.Action finishCallback);
    }
}


