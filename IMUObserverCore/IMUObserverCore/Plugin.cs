﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMUObserverCore {
    public static class Plugin {
        static BLE.IAdvertiseObserver advertiseObserver;

        static Plugin() {
            advertiseObserver = new BLE.AdvertiseObserver();
        }

        public static async Task<string[]> Scan() {
            var devices = await advertiseObserver.ScanAdvertiseDevicesAsync();
            if (devices == null || devices.Length == 0) {
                Debug.WriteLine("no device found..");
                return new string[0]; //empty
            }
            Debug.WriteLine($"found {devices.Length} devices");
            Debug.WriteLine($"DeviceId: {string.Join("/", devices.Select(x => x.UUID.ToString()))}");
            Debug.WriteLine($"BluetoothAddress :{string.Join("/", devices.Select(x => x.Address))}");
            return devices.Select(x => x.UUID).ToArray();
        }
    }
}
