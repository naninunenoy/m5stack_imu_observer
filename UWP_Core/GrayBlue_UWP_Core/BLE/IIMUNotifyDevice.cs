using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive;

namespace GrayBlueUWPCore.BLE {
    internal interface IIMUNotifyDevice : IDisposable {
        string Name { get; }
        string DeviceId { get; }
        Task<IIMUNotifyDevice> ConnectionAsync();
        void Disconnect();
        IObservable<Unit> ConnectionLostObservable();
        IObservable<byte[]> ButtonUpdateObservable();
        IObservable<byte[]> IMUUpdateObservable();
    }
}
