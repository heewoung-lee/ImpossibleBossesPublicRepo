using NetWork.NGO;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.SubItem
{
    public class UIRoomPlayerFrame : UIBase
    {
        private CharacterSelectorNgo _characterNgo;
        public CharacterSelectorNgo CharacterSelectorNgo { get => _characterNgo; }

        protected override void AwakeInit()
        {
        }

        protected override void StartInit()
        {
        }

        public void SetCharacterSelector(GameObject chracterSelecter)
        {
            _characterNgo = chracterSelecter.GetComponent<CharacterSelectorNgo>();
        }


    
    }
}
