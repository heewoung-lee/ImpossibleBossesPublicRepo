using GameManagers;
using TMPro;
using UnityEngine;
using Util;

namespace UI.Scene.SceneUI
{
    public class UILoadingProgress : UIScene
    {
        enum Texts
        {
            TitleText,
            BodyText
        }

        private TMP_Text _titleText;
        private TMP_Text _bodyText;

        protected override void AwakeInit()
        {
            base.AwakeInit();
            Bind<TMP_Text>(typeof(Texts));
            _titleText = GetText((int)Texts.TitleText);
            _bodyText = GetText((int)Texts.BodyText);
        }

        protected override void StartInit()
        {
            base.StartInit();
            SetSortingOrder((int)Define.SpecialSortingOrder.LoadingPanel);
        }

        public void ShowLoading()
        {
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            SetSortingOrder((int)Define.SpecialSortingOrder.LoadingPanel);
        }

        public void HideLoading()
        {
            gameObject.SetActive(false);
        }

        public void SetText(string titleText, string bodyText)
        {
            _titleText.text = titleText;
            _bodyText.text = bodyText;
        }

        public void ShowLoading(string titleText, string bodyText)
        {
            SetText(titleText, bodyText);
            ShowLoading();
        }
    }
}
