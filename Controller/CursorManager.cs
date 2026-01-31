using System;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Util;
using Zenject;

namespace Controller
{
    public enum CursorState
    {
        Base,
        Attack,
    }

    public interface ICursorService
    {
        void Set(CursorState state);
        void Reset();
    }

    public class CursorManager : ICursorService,IInitializable
    {
        private IResourcesServices _resourcesServices;
        private CursorState _current = CursorState.Base;
        [Inject]
        public CursorManager(IResourcesServices resourcesServices)
        {
            _resourcesServices = resourcesServices;
        }

        Texture2D _BaseCursor;
        Texture2D _AttackCursor;
        public void Initialize()
        {
            _BaseCursor = _resourcesServices.Load<Texture2D>("Art/Textures/Cursors/Base");
            _AttackCursor = _resourcesServices.Load<Texture2D>("Art/Textures/Cursors/Attack");
            ApplyCursor(CursorState.Base);
        }

        private void ApplyCursor(CursorState state)
        {
            _current = state;
            Vector2 hotspot = Vector2.zero;
            Texture2D tex;
            switch (state)
            {
                case CursorState.Base:
                    tex = _BaseCursor;
                    hotspot = new Vector2(_BaseCursor.width / 3, _BaseCursor.height / 3);
                    break;
                case CursorState.Attack:
                    tex = _AttackCursor;
                    hotspot = new Vector2(_AttackCursor.width / 4, 0);
                    break;
                default:
                    tex = _BaseCursor; // 기본값 처리
                    hotspot = new Vector2(_BaseCursor.width / 3, _BaseCursor.height / 3);
                    break;
            }
            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }
        public void Set(CursorState state)
        {
            if (state == _current)
                return;

            ApplyCursor(state);
        }
        
        public void Reset()
        {
            Set(CursorState.Base);
        }
    }
}