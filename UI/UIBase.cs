using System;
using System.Collections.Generic;
using System.Linq;
using CoreScripts;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Zenject;
using Object = UnityEngine.Object;

namespace UI
{
    public abstract class UIBase : ZenjectMonoBehaviour
    {
        protected IResourcesServices _resourcesServices;
        private Canvas _canvas;

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponent<Canvas>();
                    if (_canvas == null)
                    {
                        _canvas = GetComponentInParent<Canvas>();
                    }
                }
                return _canvas;
            }
        }
        
        [Inject]
        public void Construct(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }
        Dictionary<Type, Object[]> _bindDictionary = new Dictionary<Type,Object[]>();
        
        protected abstract void StartInit();
        protected abstract void AwakeInit();


        private void Awake()
        {
            AwakeInit();
        }
        private void Start()
        {
            StartInit();
        }
        protected void Bind<T>(Type type,Transform targetTr = null) where T : Object
        {

            if (type.IsEnum == false)
                return;

            string[] names = Enum.GetNames(type);
            Object[] objects = new Object[names.Length];
            objects = FindObjects<T>(objects, 0, names.Length, names,targetTr);

            _bindDictionary.Add(typeof(T), objects);
        }

        public void SetSortingOrder(int soringOrder)
        {
            Canvas.sortingOrder = soringOrder;
        }

        /// <summary>
        /// 26.2.18일 주석 추가.
        /// 특정 타입(T)의 객체들을 Enum 이름 기반으로 바인딩함
        /// 이미 바인딩된 타입이 있다면 기존 목록 뒤에 새로운 객체들을 추가(Append)함.
        /// (주로 상속받은 UI 클래스에서 부모의 바인딩을 유지한 채 자식 UI 요소를 추가할 때 사용)
        /// </summary>
        /// <typeparam name="T">바인딩할 컴포넌트 타입 (예: Button, Image, GameObject)</typeparam>
        /// <param name="type">추가할 오브젝트들의 이름이 정의된 Enum 타입</param>
        /// <param name="indexString">결과로 반환되는 모든 바인딩 객체(기존 + 신규)의 이름 목록</param>
        /// <param name="targetTr">어디서 부터 계층을 탐색해서 바인드된 </param>
        protected void AddBind<T>(Type type,out string[] indexString,Transform targetTr = null) where T: Object
        {
        
            //이미 해당 타입으로 바인딩된 내역이 있는 경우 (추가 모드)
            if(_bindDictionary.ContainsKey(typeof(T)))
            {
                Object[] objects = _bindDictionary[typeof(T)];
                List<string> nameList = new List<string>();
                for(int beforeIndex=0; beforeIndex < objects.Length; beforeIndex++)
                {
                    nameList.Add(objects[beforeIndex].name);
                }
                string[] names = Enum.GetNames(type);
                {
                    for (int index=0; index < names.Length; index++)
                        nameList.Add(names[index]);
                }
                Object[] newObjects = new Object[nameList.Count];
                Array.Copy(objects, newObjects, objects.Length);
                newObjects = FindObjects<T>(newObjects, objects.Length, newObjects.Length, nameList.ToArray(),targetTr);
                _bindDictionary[typeof(T)] = newObjects;
                indexString = nameList.ToArray();
            }
            else//최초 바인딩인 경우 (신규 모드)
            {
                Bind<T>(type,targetTr);
                indexString = _bindDictionary[typeof(T)].Select(bindObject=>bindObject.name).ToArray();
            }
        }

        private Object[] FindObjects<T>(Object[] objects,int startIndex,int endIndex, string[] names,Transform targetTr = null) where T : Object
        {
            Object[] newObjects = objects;


            GameObject targetGameObject;
            
            if (targetTr == null)
            {
                targetGameObject = gameObject;
            }
            else
            {
                targetGameObject = targetTr.gameObject;
            }
            
            for (int i = startIndex; i < endIndex; i++)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    newObjects[i] = Utill.FindChild(targetGameObject, names[i], true);
                }
                else
                {
                    newObjects[i] = Utill.FindChild<T>(targetGameObject, names[i], true);
                }
            }
            return newObjects;
        }



        protected T Get<T>(int idx) where T : Object
        {
            Object[] objects = null;

            if(_bindDictionary.TryGetValue(typeof(T),out objects) == false)
            {
                UtilDebug.LogError($"not Found Object{typeof(T)}");
                return null;
            }
            return objects[idx] as T;
        }


        protected TMP_Text GetText(int idx) => Get<TMP_Text>(idx);

        protected Button GetButton(int idx) => Get<Button>(idx);

        protected Image GetImage(int idx) => Get<Image>(idx);

        protected GameObject GetObject(int idx) => Get<GameObject>(idx);

        // UI_Base는 모든 UI들이 상속받는 프레임워크
        //딕셔너리를 통해 UI_Base의 Bind타입들을 키로 저장하고, BIND타입안에 있는 이름과 같은 객체들을 배열로 저장한다.

        //1) Bind를 통해 Enum Type을 인자로 받고 Enum type안에 있는 인자들을 이름으로 받는다.
        //해당타입이 Gameobject라면 해당객체를 저장하고, 아니라면, 제네릭 T타입을 돌려받는다.
        //2) Get을 통해 딕셔너리에 저장된 타입을 Enum의 이름으로 꺼내온다.
        //3) BindEvent를 통해 해당객체에 이벤트 핸들러를 달아준다.

        public void BindEvent(GameObject go,Action<PointerEventData> action,Define.UIEvent mouseEvent = Define.UIEvent.LeftClick)
        {
            UIEventHandler evt = _resourcesServices.GetOrAddComponent<UIEventHandler>(go);
            switch (mouseEvent)
            {
                case Define.UIEvent.LeftClick:
                    evt.OnLeftClickEvent += action;
                    break;
                case Define.UIEvent.RightClick:
                    evt.OnRightClickEvent += action;
                    break;
                case Define.UIEvent.Drag:
                    evt.OnDragEvent += action;
                    break;
                case Define.UIEvent.DragBegin:
                    evt.OnBeginDragEvent += action;
                    break;
                case Define.UIEvent.DragEnd:
                    evt.OnEndDragEvent += action;
                    break;
                case Define.UIEvent.PointerEnter:
                    evt.OnPointerEnterEvent += action;
                    break;
                case Define.UIEvent.PointerExit:
                    evt.OnPointerExitEvent += action;
                    break;
            }
        }
        public void UnBindEvent(GameObject go,Action<PointerEventData> action,Define.UIEvent mouseEvent = Define.UIEvent.LeftClick)
        {
            UIEventHandler evt = _resourcesServices.GetOrAddComponent<UIEventHandler>(go);
            switch (mouseEvent)
            {
                case Define.UIEvent.LeftClick:
                    evt.OnLeftClickEvent -= action;
                    break;
                case Define.UIEvent.RightClick:
                    evt.OnRightClickEvent -= action;
                    break;
                case Define.UIEvent.Drag:
                    evt.OnDragEvent -= action;
                    break;
                case Define.UIEvent.DragBegin:
                    evt.OnBeginDragEvent -= action;
                    break;
                case Define.UIEvent.DragEnd:
                    evt.OnEndDragEvent -= action;
                    break;
                case Define.UIEvent.PointerEnter:
                    evt.OnPointerEnterEvent -= action;
                    break;
                case Define.UIEvent.PointerExit:
                    evt.OnPointerExitEvent -= action;
                    break;
            }
        }
        protected Vector2 GetUISize(GameObject uiObject)
        {
            RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
            RectTransform parentRect = rectTransform.parent as RectTransform;

            if (parentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            Vector2 size = rectTransform.rect.size;
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            return size;
        }

        protected Vector2 GetUIScreenPosition(RectTransform rectTr)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTr.position);
            return screenPos;
        }

    }
}