namespace Tracking4All
{
    /// <summary>
    /// The camera settings to configure mpu.
    /// </summary>
    [System.Serializable]
    public class MPUCameraSettings
    {
        public IntSetting cameraIndex; // 0
        public IntSetting width; // 1920
        public IntSetting height; // 1080
        public FloatSetting frameRate; // 30
        public BoolSetting backFacingCamera; // true,  relevant to front facing mobile cameras and whenever you wanna flip

        public Mediapipe.Unity.ImageSource.ResolutionStruct Resolution 
            => new Mediapipe.Unity.ImageSource.ResolutionStruct(width.Value, height.Value, frameRate.Value);
    }
}