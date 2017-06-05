﻿using Newtonsoft.Json;
using QuickShare.DevicesListManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuickShare.Common.Service
{
    public static class DevicesLoader
    {
        public static async Task<IEnumerable<NormalizedRemoteSystem>> GetAndroidDevices(string userId)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{Constants.ServerAddress}/api/User/{userId}/Devices/Android");
            var responseText = await response.Content.ReadAsStringAsync();

            try
            {
                var devices = JsonConvert.DeserializeObject<List<Models.Device>>(responseText);

                var output = from d in devices
                             select new NormalizedRemoteSystem
                             {
                                 Id = d.DeviceID,
                                 DisplayName = d.FriendlyName,
                                 Kind = "QS_Android",
                                 Status = NormalizedRemoteSystemStatus.Available,
                                 IsAvailableByProximity = false,
                                 IsAvailableBySpatialProximity = false,
                             };

                return output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAndroidDevices: {ex.Message}");
                Debug.WriteLine($"Server returned text was '{responseText}'");
                return new List<NormalizedRemoteSystem>();
            }
        }

        public static async Task<bool> WakeAndroidDevices(string userId)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{Constants.ServerAddress}/api/User/{userId}/TryWakeAll");
            var responseText = await response.Content.ReadAsStringAsync();

            if (responseText != "1, done")
            {
                Debug.WriteLine($"Received unexpected message from TryWakeAll: '{responseText}'");
                return false;
            }

            return true;
        }

        public static async Task<bool> RequestMessageCarrier(string userId, string deviceId, IEnumerable<string> whosNotMe)
        {
            var httpClient = new HttpClient();
            var jsonData = JsonConvert.SerializeObject(whosNotMe);
            var response = await httpClient.PostAsync($"{Constants.ServerAddress}/api/User/{userId}/{deviceId}/StartCarrierService", new StringContent(jsonData, Encoding.UTF8, "application/json"));
            var responseText = await response.Content.ReadAsStringAsync();

            if (responseText != "1, done")
            {
                Debug.WriteLine($"Received unexpected message from StartCarrierService: '{responseText}'");
                return false;
            }

            return true;
        }
    }
}