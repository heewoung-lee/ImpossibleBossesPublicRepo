namespace GameManagers.SoundManagement
{
    public enum UICommonSoundCueId
    {
        None,
        Hover,
        Click,
        Close,
    }

    public static class UICommonSoundCueIds
    {
        public static string GetId(UICommonSoundCueId cueId)
        {
            if (cueId == UICommonSoundCueId.None)
            {
                return null;
            }

            return cueId.ToString();
        }
    }
}
