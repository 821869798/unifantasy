using System.Collections.Generic;

namespace UniFan
{
    public class ManagerCenter : Singleton<ManagerCenter>
    {

        private List<ManagerBase> _managers;

        protected override void Initialize()
        {
            _managers = new List<ManagerBase>();
            MonoDriver.Instance.updateHandle += Update;
        }

        public void BindManage(ManagerBase t)
        {
            if (t == null)
                return;
            for (int i = 0; i < _managers.Count; i++)
            {
                var manager = _managers[i];
                if (t.managerPriority < manager.managerPriority)
                {
                    _managers.Insert(i, t);
                    return;
                }
                if (t.managerPriority == manager.managerPriority)
                {
                    throw new System.Exception("there are two manager priority is equal");
                }
            }
            _managers.Add(t);
        }
        public bool RemoveManager<T>(T t) where T : ManagerBase, new()
        {
            return _managers.Remove(t);
        }

        public void Update(float delteTime)
        {
            foreach (var manager in _managers)
            {
                if (manager != null)
                {
                    manager.OnUpdate(delteTime);
                }
            }
        }
    }

}

