namespace Assets.Scripts.Remote
{
    public class RemoteTablet 
    {
        private readonly RemoteTargetTemperature _remoteTargetTemperature;
        private readonly RemoteWindow _remoteWindow;

        public RemoteTablet(RemoteTargetTemperature remoteTargetTemperature, RemoteWindow remoteWindow)
        {
            _remoteTargetTemperature = remoteTargetTemperature;
            _remoteWindow = remoteWindow;
        }

        public float GetTargetTemperature()
        {
            return _remoteTargetTemperature.GetState();
        }

        public void SetTargetTemperatur(float value)
        {
            _remoteTargetTemperature.SetState(value);
        }

        public bool GetWindowPosition()
        {
            return _remoteWindow.GetState();
        }

        public void SetWindowPosition(bool value)
        {
            _remoteWindow.SetState(value);
        }

        public RemoteWindow getRemoteWindow()
        {
            return _remoteWindow;
        }

    }
}
