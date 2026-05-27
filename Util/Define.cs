using UnityEngine;

namespace Util
{
    public class Define
    {
        public static readonly Vector3 DefaultQuarterviewPosition = new Vector3(0, 7, -6);
        public static readonly Quaternion DefaultQuarterviewRotation = Quaternion.Euler(new Vector3(45, 0, 0));
        public static readonly int MaxPlayer = 8;
        public const string Applicationname = "ItemDataSheet";
        

        public enum SpecialSortingOrder
        {
            DragImage = 30,
            Description = 50,
            LoadingScreen = 70,
            LoadingPanel = 100,

        }
        public enum PlayerClass
        {
            Archer,
            Fighter,
            Mage,
            Monk,
            Necromancer,
        }
        public enum ControllerLayer
        {
            Player,
            AnotherPlayer,
            Moster
        }
        public enum WorldObject
        {
            Unknown,
            Player,
            Monster,
            Boss
        }
        public enum SceneName
        {
            Unknown,
            LoginScene,
            LobbyScene,
            RoomScene,
            GamePlayScene,
            FirstBossScene,
            SecondBossScene,
            ThirdBossScene,
            LoadingScene,
            NetworkLoadingScene,
            PreLoadingScene
        }
        public enum Layer
        {
            Block = 8,
            Monster = 9,
            Npc = 10
        }
        public enum Sound
        {
            SFX,
            BGM
        }
        public enum UIEvent
        {
            LeftClick,
            RightClick,
            DragBegin,
            Drag,
            DragEnd,
            PointerEnter,
            PointerExit
        }
        public enum CameraMode
        {
            QuarterView,
        }

        public enum BossID
        {
            Golem = 101,
            DarkWizard = 102,
            RedDragon = 103,
        }
        public enum ControllerType
        {
            Player,
            Camera,
            UI
        }
    }
}
