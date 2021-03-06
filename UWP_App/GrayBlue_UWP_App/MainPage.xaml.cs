using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace IMUObserverApp {
    using Core = GrayBlueUWPCore;
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page, Core.IConnectionDelegate, Core.INotifyDelegate {

        public MainPage() {
            this.InitializeComponent();

            Task.Run(async () => {
                var canUse = await Core.Plugin.Instance.CanUseBle();
                if (canUse) {
                    Debug.WriteLine("BLE Available!");
                    var deviceIds = await Core.Plugin.Instance.Scan();
                    Debug.WriteLine($"found {deviceIds.Length} devices. {string.Join(",", deviceIds)}");
                    if (deviceIds.Length > 0) {
                        var deviceId = deviceIds[0];
                        Core.Plugin.Instance.ConnectTo(deviceId, this, this);
                    }
                } else {
                    Debug.WriteLine("BLE Unavailable..");
                }
            });
        }

        public void OnIMUDataUpdate(string deviceId, float[] acc, float[] gyro, float[] mag, float[] quat) {
            Debug.WriteLine($"OnIMUDataUpdate {deviceId}");
            Debug.WriteLine($" Time=({DateTime.Now.ToString("HH:mm:ss.fffff")})");
            Debug.WriteLine($"  Acc=({string.Join(", ", acc.Select(x => x.To3FixString()))})");
            Debug.WriteLine($" Gyro=({string.Join(", ", gyro.Select(x => x.To3FixString()))})");
            Debug.WriteLine($"  Mag=({string.Join(", ", mag.Select(x => x.To3FixString()))})");
            Debug.WriteLine($" Quat=({string.Join(", ", quat.Select(x => x.To3FixString()))})");
        }

        public void OnButtonPush(string deviceId, string buttonName) {
            Debug.WriteLine($"OnButtonPush {buttonName} {deviceId}");
        }

        public void OnButtonRelease(string deviceId, string buttonName, float pressTime) {
            Debug.WriteLine($"OnButtonRelease {buttonName} {pressTime} {deviceId}");
        }

        public void OnConnectDone(string deviceId) {
            Debug.WriteLine($"OnConnectDone {deviceId}");
        }

        public void OnConnectFail(string deviceId) {
            Debug.WriteLine($"OnConnectFail {deviceId}");
        }

        public void OnConnectLost(string deviceId) {
            Debug.WriteLine($"OnConnectLost {deviceId}");
        }
    }

    static class FloatStringEx {
        public static string To3FixString(this float x) {
            x = Math.Min(9999.999F, x);
            x = Math.Max(x, -9999.999F);
            return x.ToString("F3").PadLeft(9, ' ');
        }
    }
}
