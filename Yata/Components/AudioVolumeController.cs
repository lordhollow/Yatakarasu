using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Yata.Components
{
    /// <summary>
    /// オーディオボリュームコントローラー(デフォルトデバイス＝タスクバーで見えるやつ）の音量参照と変更
    /// </summary>
    class AudioVolumeController : IMMNotificationClient, IDisposable
    {
        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        /// <remarks>
        /// 更新中はMulte/Volumeの参照処理を排他しないと無効になったデバイスを参照する瞬間に死ぬ。
        /// </remarks>
        object lockObject = new object();

        /// <summary>
        /// 走査対象とするデバイスのロール。
        /// </summary>
        /// <remarks>
        /// Consoleでも結果変わらなさそう。
        /// Androidみたいに通知/音楽/通話のバーが分かれているものを想定しているような気がする。
        /// 少なくともこれを描いているPCにはConsoleとMultimediaが必ずセットになっているから気にしない。
        /// </remarks>
        Role TargetRole = Role.Multimedia;

        /// <summary>
        /// デバイス列挙子
        /// </summary>
        IMMDeviceEnumerator deviceEnumerator = null;
        /// <summary>
        /// スピーカー
        /// </summary>
        IAudioEndpointVolume renderEndpoint = null;
        /// <summary>
        /// マイク
        /// </summary>
        IAudioEndpointVolume captureEndpoint = null;

        /// <summary>
        /// 初期化
        /// </summary>
        public AudioVolumeController()
        {
            deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
            GetDefaultEndpoints();
        }


        public void Dispose()
        {
            if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            if (renderEndpoint != null) Marshal.ReleaseComObject(renderEndpoint);
            if (captureEndpoint != null) Marshal.ReleaseComObject(captureEndpoint);
        }

        /// <summary>
        /// レンダー(スピーカー)デバイスの有無
        /// </summary>
        public bool HasRender { get { lock (lockObject) { return renderEndpoint != null; } } }

        /// <summary>
        /// レンダ名
        /// </summary>
        public string RenderName { get; private set; }

        /// <summary>
        /// キャプチャー(マイク)デバイスの有無
        /// </summary>
        public bool HasCapture { get { lock (lockObject) { return captureEndpoint != null; } } }

        /// <summary>
        /// キャプチャ名
        /// </summary>
        public string CaptureName { get; private set; }

        /// <summary>
        /// ミュートの参照と設定
        /// </summary>
        public bool Mute
        {
            get { lock (lockObject) { return GetMute(renderEndpoint); } }
            set { lock (lockObject) { SetMute(renderEndpoint, value); } }
        }

        /// <summary>
        /// 音量の参照と設定(%)。デバイスがないときは-1が得られる。
        /// </summary>
        public float Volume
        {
            get { lock (lockObject) { return GetVolume(renderEndpoint); } }
            set { lock (lockObject) { SetVolume(renderEndpoint, value); } }
        }

        /// <summary>
        /// ミュートの参照と設定
        /// </summary>
        public bool CaptureMute
        {
            get { lock (lockObject) { return GetMute(captureEndpoint); } }
            set { lock (lockObject) { SetMute(captureEndpoint, value); } }
        }

        /// <summary>
        /// 音量の参照と設定(%)。デバイスがないときは-1が得られる。
        /// </summary>
        public float CaptureVolume
        {
            get => GetVolume(captureEndpoint);
            set => SetVolume(captureEndpoint, value);
        }
        
        /// <summary>
        /// ミュート状態の取得
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        private bool GetMute(IAudioEndpointVolume ep)
        {
            //ここでlockせず上位でlockしているのはlock中にepのオブジェクトそのものが変化し、無効になるケースがあるため。他も同様。
            //ものがないときは聞こえないんだからミュートです
            if (ep == null) return true;
            bool mute;
            ep.GetMute(out mute);
            return mute;
        }

        /// <summary>
        /// ミュート状態の設定
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="mute"></param>
        private void SetMute(IAudioEndpointVolume ep, bool mute)
        {
            ep?.SetMute(mute, Guid.Empty);

        }
        
        /// <summary>
        /// ボリュームの参照
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        private float GetVolume(IAudioEndpointVolume ep)
        {
            if (ep == null) return -1;
            //スカラでないバージョンは値がdbになっているので使いにくい。スカラは0～1の値なので100で倍する。
            float volume;
            ep.GetMasterVolumeLevelScalar(out volume);
            volume *= 100;
            return volume;
        }

        /// <summary>
        /// ボリュームの設定
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="value"></param>
        private void SetVolume(IAudioEndpointVolume ep, float value)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            ep?.SetMasterVolumeLevelScalar(value / 100.0f, Guid.Empty);
        }

        /// <summary>
        /// デフォルトエンドポイントを持っておく
        /// </summary>
        private void GetDefaultEndpoints()
        {
            lock (lockObject)
            {
                if (renderEndpoint != null) Marshal.ReleaseComObject(renderEndpoint);
                if (captureEndpoint != null) Marshal.ReleaseComObject(captureEndpoint);

                string n;
                renderEndpoint = GetDefaultEndpoint(DataFlow.Render, out n);
                RenderName = n;
                captureEndpoint = GetDefaultEndpoint(DataFlow.Capture, out n);
                CaptureName = n;
            }
        }

        /// <summary>
        /// デフォルトエンドポイントの取得
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        private IAudioEndpointVolume GetDefaultEndpoint(DataFlow flow, out string deviceName)
        {
            IAudioEndpointVolume endpointInterface;
            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;

            IMMDevice device = null;
            deviceName = null;
            try
            {
                deviceEnumerator.GetDefaultAudioEndpoint(flow, TargetRole, out device);
                device.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out object endpointObject);
                endpointInterface = (IAudioEndpointVolume)endpointObject;

                //さらに、デバイスからプロパティを取得する。
                IPropertyStore propStore;
                device.OpenPropertyStore(StorageAccessMode.Read, out propStore);
                var store = new PropertyStore(propStore);
                var ifFamily = store[PropertyKeys.PKEY_DeviceInterface_FriendlyName].Value;
                var familty = store[PropertyKeys.PKEY_Device_FriendlyName].Value;
                deviceName = $"{ifFamily} ({familty})";
            }
            catch
            {
                endpointInterface = null;
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
            }
            return endpointInterface;
        }


        #region IMMNotificationClient

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId)
        {
            if (role == TargetRole)
            {
                GetDefaultEndpoints();
            }
        }
        public void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId) { }
        public void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId) { }
        public void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.I4)] DeviceState newState) { }
        public void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key) { }

        #endregion
    }
}
