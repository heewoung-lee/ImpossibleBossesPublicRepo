using System;
using BehaviorDesigner.Runtime;
using Data.DataType.ItemType.Interface;
using GameManagers;
using GameManagers.Interface;
using GameManagers.Interface.UIManager;
using UI;
using UI.Popup.PopupUI;
using UI.SubItem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Util
{
    public static class UIExtension
    {
        public static Color SetGradeColor(this MaskableGraphic graphic, Color setColor)
        {
            if (graphic != null)
            {
                graphic.color = setColor; // MaskableGraphic의 color 속성 설정
                return graphic.color; // 설정된 색상을 반환
            }
            return Color.white; // 그래픽이 null이면 기본 색상 반환
        }
        public static bool TryGetComponentInsChildren<T>(this GameObject origin, out T[] getComponent) where T : UnityEngine.Object
        {
            getComponent = origin.GetComponentsInChildren<T>();
            if (getComponent == null)
                return false;
            else
                return true;
        }
        public static bool TryGetComponentInChildren<T>(this GameObject origin, out T getComponent) where T : UnityEngine.Object
        {
            getComponent = origin.GetComponentInChildren<T>();
            if (getComponent == null)
                return false;
            else
                return true;
        }


        public static T FindChild<T>(this GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            return Utill.FindChild<T>(go, name, recursive);
        }


        public static T FindParantComponent<T>(this Transform currentTr)
        {
            Transform parent = currentTr.parent;
            while (parent != null)
            {
                T component = parent.GetComponent<T>();
                if (component == null)
                {
                    parent = parent.parent;
                }
                else
                {
                    return component;
                }
            }
            return default(T);
        }
        public static Color HexCodetoConvertColor(this string hexCode)
        {
            if(hexCode.Contains("#")== false)
            {
                hexCode = "#" + hexCode;
            }

            if(UnityEngine.ColorUtility.TryParseHtmlString(hexCode,out Color color))
            {
                return color;
            }
            return Color.white;   
        }

        public static UIAlertPopupBase AfterAlertEvent(this UIAlertPopupBase dialog,UnityAction action)
        {
            dialog.SetCloseButtonOverride(action);
            return dialog;
        }

        public static UIAlertPopupBase AlertSetText(this UIAlertPopupBase dialog, string titleText,string bodyText)
        {
            dialog.SetText(titleText, bodyText);
            return dialog;
        }


        public static Image ImageEnable(this Image image,bool isLobbyLoading)
        {
            image.enabled = isLobbyLoading;
            return image;
        }
    }
}