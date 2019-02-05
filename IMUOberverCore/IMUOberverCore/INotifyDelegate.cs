﻿using System;

namespace IMUOberverCore {
    interface INotifyDelegate {
        void OnButtonPush(string uuid, string buttonName);
        void OnButtonRelease(string uuid, string buttonName, float pressTime);
    }
}