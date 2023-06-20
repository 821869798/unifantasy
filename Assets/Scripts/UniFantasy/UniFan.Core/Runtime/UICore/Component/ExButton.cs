using UnityEngine;
using UnityEngine.UI;

namespace UniFan
{
    [AddComponentMenu("UIEx/ExButton")]
    public class ExButton : Button
    {
        [SerializeField]
        private bool _isBackButton;

        public bool isBackButton
        {
            set { _isBackButton = value; }
            get { return _isBackButton; }
        }
    }
}