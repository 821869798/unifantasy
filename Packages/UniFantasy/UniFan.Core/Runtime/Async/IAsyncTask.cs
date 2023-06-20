using System;
using System.Collections.Generic;


namespace UniFan
{
    public interface IAsyncTask
    {
        void DoAsyncTask();
        void RegisterAsyncTaskCallback(Action<bool, IAsyncTask> listener);
        void RemoveAsyncTaskCallback(Action<bool, IAsyncTask> listener);
    }

}
