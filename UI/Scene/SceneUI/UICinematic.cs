using UnityEngine;
using Util;

namespace UI.Scene.SceneUI
{
    public class UICinematic : UIScene
    {
        private enum Rects
        {
            Up,
            Down
        }

        private RectTransform _upRect;
        private RectTransform _downRect;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<RectTransform>(typeof(Rects));
            _upRect = Get<RectTransform>((int)Rects.Up);
            _downRect = Get<RectTransform>((int)Rects.Down);
            SetBarHeight(0f);
        }

        protected override void StartInit()
        {
            base.StartInit();
            SetSortingOrder((int)Define.SpecialSortingOrder.LoadingScreen + 1);
        }

        private void SetBarHeight(float height)
        {
            Vector2 upSizeDelta = _upRect.sizeDelta;
            upSizeDelta.y = height;
            _upRect.sizeDelta = upSizeDelta;

            Vector2 downSizeDelta = _downRect.sizeDelta;
            downSizeDelta.y = height;
            _downRect.sizeDelta = downSizeDelta;
        }
    }
}
