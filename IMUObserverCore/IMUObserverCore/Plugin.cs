using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMUObserverCore {
    public static class Plugin {
        static BLE.IAdvertiseObserver advertiseObserver;
        static IDictionary<string, BLE.IIMUNotifyDevice> DeviceDict;

        static Plugin() {
            advertiseObserver = new BLE.AdvertiseObserver();
            DeviceDict = new Dictionary<string, BLE.IIMUNotifyDevice>();
        }

        public static async Task<string[]> Scan() {
            // scan devices
            Debug.WriteLine("scanning..");
            var devices = await advertiseObserver.ScanAdvertiseDevicesAsync();
            if (devices == null || devices.Length == 0) {
                Debug.WriteLine("no device found..");
                return new string[0]; //empty
            }
            Debug.WriteLine($"found {devices.Length} devices");
            // read deviceId
            var deviceIds = await Task.WhenAll(devices.Select(async x => await x.GetDeviceIdAsync()));
            Debug.WriteLine($"DeviceId: {string.Join("/", deviceIds)}");
            return deviceIds.ToArray();
        }

        public static async Task<bool> ConnectTo(string deviceId, IConnectionDelegate connectionDelegate, INotifyDelegate notifyDelegate) {
            BLE.IIMUNotifyDevice device = new BLE.IMUDevice(deviceId);
            try {
                device = await device.ConnectionAsync();
            } catch (BLE.BLEException e) {
                Debug.WriteLine($"{deviceId} connect failed. {e.Message}");
                connectionDelegate?.OnConnectFail(deviceId);
                device.Dispose();
                return false;
            }
            device.ConnectionLostObservable()
                  .Subscribe(_ => { connectionDelegate?.OnConnectLost(deviceId); });
            device.ButtonUpdateObservable()
                  .Subscribe(data => {
                      bool press = (data[0] != 0);
                      char name = (char)data[1];
                      short ms = (short)((data[3] << 8) + data[2]);
                      float time = ms / 1000.0F;
                      if (press) {
                          notifyDelegate?.OnButtonPush(deviceId, name.ToString());
                      } else {
                          notifyDelegate?.OnButtonRelease(deviceId, name.ToString(), time);
                      }
                  });
            device.IMUUpdateObservable()
                  .Subscribe(data => {
                      var acc = new float[3] {
                          BitConverter.ToSingle(data, 0),
                          BitConverter.ToSingle(data, 4),
                          BitConverter.ToSingle(data, 8)
                      };
                      var gyro = new float[3] {
                          BitConverter.ToSingle(data, 12),
                          BitConverter.ToSingle(data, 16),
                          BitConverter.ToSingle(data, 20)
                      };
                      var mag = new float[3] {
                          BitConverter.ToSingle(data, 24),
                          BitConverter.ToSingle(data, 28),
                          BitConverter.ToSingle(data, 32)
                      };
                      var quat = new float[4] {
                          BitConverter.ToSingle(data, 36),
                          BitConverter.ToSingle(data, 40),
                          BitConverter.ToSingle(data, 44),
                          BitConverter.ToSingle(data, 48)
                      };
                      notifyDelegate?.OnIMUDataUpdate(deviceId, acc, gyro, mag, quat);
                  });
            connectionDelegate?.OnConnectDone(deviceId);
            if (DeviceDict.ContainsKey(deviceId)) {
                // overwrite
                DeviceDict[deviceId].Dispose();
                DeviceDict[deviceId] = device;
            } else {
                DeviceDict.Add(deviceId, device);
            }
            return true;
        }

        public static void DisconnectTo(string deviceId) {
            if (!DeviceDict.ContainsKey(deviceId)) {
                Debug.Fail($"{deviceId} is not exist");
                return;
            }
            DeviceDict[deviceId].Disconnect();
            DeviceDict.Remove(deviceId);
        }

        public static void DisconnectAllDevices() {
            foreach(var device in DeviceDict.Values) {
                device.Disconnect();
            }
            DeviceDict.Clear();
        }

        public static void Dispose() {
            foreach (var device in DeviceDict.Values) {
                device.Dispose();
            }
            advertiseObserver.Dispose();
            DeviceDict.Clear();
        }
    }
}
