using System;
using System.Collections.Generic;
using System.Linq;
using Data.DataType.ItemType.Interface;
using DataType.Item;
using GameManagers;
using GameManagers.Interface.UIManager;
using UI.Popup;
using UI.Scene;
using UI.Scene.SceneUI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Zenject;

namespace UI.SubItem
{
    public struct IteminfoStruct : INetworkSerializable
    {
        public int ItemNumber;

        // [변경] 생성자: IItem 대신 int(ID)를 받음
        public IteminfoStruct(int itemNumber)
        {
            ItemNumber = itemNumber;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ItemNumber);
        }
    }

    public abstract class UIItemComponent : UIBase
    {
        private readonly float _itemVisibleValue = 0.5f;
        [Inject] private IUIManagerServices _uiManagerServices;

        protected ItemDataSO _itemData;

        protected Image _itemIconSourceImage;
        protected UIItemDragImage _ui_dragImageIcon;
        protected UIDescription _decriptionObject;
        protected bool _isDragging = false;

        public int ItemNumber => _itemData != null ? _itemData.itemNumber : 0;
        public string ItemName => _itemData != null ? _itemData.dataName : "";


        public abstract RectTransform ItemRectTr { get; }

        public UIItemDragImage UIDragImageIcon
        {
            get
            {
                if (_ui_dragImageIcon == null)
                {
                    if (_uiManagerServices.Try_Get_Scene_UI<UIItemDragImage>(out UIItemDragImage itemDragIamge) == true)
                    {
                        _ui_dragImageIcon = itemDragIamge;
                    }
                }

                return _ui_dragImageIcon;
            }
        }


        protected override void StartInit()
        {
            _decriptionObject = _uiManagerServices.Get_Scene_UI<UIDescription>();
        }

        public virtual void InitializeItem(ItemDataSO data)
        {
            _itemData = data;

            if (_itemIconSourceImage != null && data.icon != null)
            {
                _itemIconSourceImage.sprite = data.icon;
            }
        }


        public void ShowDescription(PointerEventData eventdata)
        {
            if (_isDragging || _itemData == null)
                return;

            _decriptionObject.UI_DescriptionEnable();

            _decriptionObject.DescriptionWindow.transform.position
                = _decriptionObject.SetDecriptionPos(transform, ItemRectTr.rect.width, ItemRectTr.rect.height);

            _decriptionObject.SetValue(_itemData);
            
            _decriptionObject.SetSortingOrder((int)Define.SpecialSortingOrder.Description);
            //그냥 고정값으로 수정함. 이전에 방식들이 정교하긴한데, 성능을 잡아먹어서 안되겠음
        }

        protected override void ZenjectEnable()
        {
            base.ZenjectEnable();
            BindEvent(gameObject, ShowDescription, Define.UIEvent.PointerEnter);
            BindEvent(gameObject, CloseDescription, Define.UIEvent.PointerExit);
            BindEvent(gameObject, ItemRightClick, Define.UIEvent.RightClick);
            BindEvent(gameObject, GetDragBegin, Define.UIEvent.DragBegin);
            BindEvent(gameObject, DraggingItem, Define.UIEvent.Drag);
            BindEvent(gameObject, GetDragEnd, Define.UIEvent.DragEnd);
        }

        protected override void ZenjectDisable()
        {
            base.ZenjectDisable();
            if (UIDragImageIcon == null) return;

            if (UIDragImageIcon.IsDragImageActive == true)
            {
                UIDragImageIcon.SetItemImageDisable();
            }

            UnBindEvent(gameObject, ShowDescription, Define.UIEvent.PointerEnter);
            UnBindEvent(gameObject, CloseDescription, Define.UIEvent.PointerExit);
            UnBindEvent(gameObject, ItemRightClick, Define.UIEvent.RightClick);
            UnBindEvent(gameObject, GetDragBegin, Define.UIEvent.DragBegin);
            UnBindEvent(gameObject, DraggingItem, Define.UIEvent.Drag);
            UnBindEvent(gameObject, GetDragEnd, Define.UIEvent.DragEnd);
            RevertImage();
        }

        protected void RevertImage()
        {
            // 알파값 복구
            if (_itemIconSourceImage != null)
            {
                Color c = _itemIconSourceImage.color;
                _itemIconSourceImage.color = new Color(c.r, c.g, c.b, 1f);
            }

            _isDragging = false;
            UIDragImageIcon.SetItemImageDisable();
        }

        public void CloseDescription(PointerEventData eventdata) => CloseDescription();

        protected void CloseDescription()
        {
            _decriptionObject.UI_DescriptionDisable();
            _decriptionObject.SetdecriptionOriginPos();
        }

        public virtual void ItemRightClick(PointerEventData eventdata)
        {
            if (_decriptionObject.gameObject.activeSelf)
            {
                _decriptionObject.UI_DescriptionDisable();
            }
        }

        public void GetDragBegin(PointerEventData eventData)
        {
            UIDragImageIcon.SetImageSprite(_itemIconSourceImage.sprite);
            UIDragImageIcon.SetItemImageEnable();

            // 드래그 시작 시 본체 아이콘 투명하게
            Color c = _itemIconSourceImage.color;
            _itemIconSourceImage.color = new Color(c.r, c.g, c.b, 0f);

            UIDragImageIcon.SetImageSpriteColorAlpah(_itemVisibleValue);
            _isDragging = true;
        }

        public void DraggingItem(PointerEventData eventData)
        {
            UIDragImageIcon.SetDragImagePosition(eventData.position);
        }

        public abstract void GetDragEnd(PointerEventData eventData);
    }
}