// AdapterSettingsProvider
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    [System.Serializable]
    public class AdapterSettingsProvider : InterfaceProvider<IAdapterSettings>, IAdapterSettings
    {
        public bool PerspectiveFlip => Provider.PerspectiveFlip;
        public bool Mirror => Provider.Mirror;
    }
}