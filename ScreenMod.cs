using UnityEngine;
using VoxelTycoon.Modding;
using UnityEngine.UI;
using System.IO;
using VoxelTycoon;

namespace ScreenHomeRegionModForVoxelTycoon
{
    public class ScreenMod : Mod
    {
        Transform ScreenButtonTransform;
        Transform CameraTransform;

        bool isActivate = false;
        float screenRate = 60f;
        float nextScreenTime = 0f;

        int resWidth = 3840;
        int resHeight = 2160;

        float fogZoneHeight = 568f;
        Quaternion newRot = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f);

        protected override void OnGameStarted()
        {
            ScreenButtonTransform = GameObject.Find("ModernGameUI/Camera").transform;
            ScreenButtonTransform = Object.Instantiate(ScreenButtonTransform, ScreenButtonTransform.parent);
            ScreenButtonTransform.localPosition += Vector3.right * 40;
            ScreenButtonTransform.GetComponentInChildren<Button>().onClick.AddListener(delegate { Switch(); });

            CameraTransform = CameraController.Current.Camera.transform;
        }

        protected override void OnLateUpdate()
        {
            if (isActivate && Time.time > nextScreenTime)
            {
                nextScreenTime = Time.time + screenRate;

                var lastPos = CameraTransform.position;
                var lastRot = CameraTransform.rotation;

                var home = RegionManager.Current.HomeRegion;
                float dis = Xz.Distance(home.Center, new Xz(home.ColliderPath[0])) * 1.5f;
                Vector3 newPos = new Vector3(home.Center.X, fogZoneHeight, home.Center.Z);

                CameraTransform.SetPositionAndRotation(newPos, newRot);

                CreateScreen(CameraTransform.GetComponent<Camera>());

                CameraTransform.SetPositionAndRotation(lastPos, lastRot);
            }
        }

        void Switch()
        {
            isActivate = !isActivate;
        }

        private void CreateScreen(Camera camera)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            //TODO asyng fileWrite...
            File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }

        public static string ScreenShotName(int width, int height)
        {
            return string.Format("{0}/screen_{1}x{2}_{3}.png",
                                 Application.dataPath,
                                 width, height,
                                 System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }
    }
}
