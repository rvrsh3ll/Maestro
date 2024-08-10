﻿using System.Threading;
using System.Threading.Tasks;

namespace Maestro
{
    public class ExecIntuneScriptCommand
    {
        public static async Task Execute(CommandLineOptions options, LiteDBHandler database)
        {
            // Authenticate and get an access token for Intune
            var intuneClient = new IntuneClient();
            intuneClient = await IntuneClient.InitAndGetAccessToken(options, database);

            string deviceId = options.Id;
            IntuneDevice device = await intuneClient.GetDevice(deviceId, database: database);
            if (device is null) return;

            string filterId = await intuneClient.NewDeviceAssignmentFilter(device.DeviceName);
            if (filterId is null) return;

            string scriptId = await intuneClient.NewScriptPackage(options.Name, options.Script);
            if (scriptId is null) return;

            await intuneClient.NewDeviceManagementScriptAssignment(filterId, scriptId);
            Logger.Info("Script assignment created, waiting 10 seconds before requesting device sync");
            Thread.Sleep(10000);
            await intuneClient.SyncDevice(deviceId, database, skipDeviceLookup: true);
        }
    }
}