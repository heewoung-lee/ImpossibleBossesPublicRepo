using GameManagers.Interface.GoogleAuthLogin;
using GameManagers.Interface.ResourcesManager;
using GameManagers.ResourcesEx;
using UnityEngine;
using Zenject;

namespace Data
{
   
    public class GoogleAuthLogin: IGoogleAuthLoginLoader
    {
        [Inject] private IResourcesServices _resourcesServices;
        public TextAsset[] LoadGoogleAuthJsonFiles()
        {
            TextAsset[] jsonFiles = _resourcesServices.LoadAll<TextAsset>("GoogleLoginData");
            return jsonFiles;
        }

        public GoogleLoginWrapper ParseJsontoGoogleAuth(TextAsset[] jsonFile)
        {
            foreach (TextAsset file in jsonFile)
            {
                if (file.text.Contains("\"installed\"") == false)
                    continue;

                return JsonUtility.FromJson<GoogleLoginWrapper>(file.text);
            }

            return default(GoogleLoginWrapper);
        }
    }
}