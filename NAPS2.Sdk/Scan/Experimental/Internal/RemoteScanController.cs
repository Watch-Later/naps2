﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class RemoteScanController : IRemoteScanController
    {
        private readonly IScanDriverFactory scanDriverFactory;
        private readonly IRemotePostProcessor remotePostProcessor;

        public RemoteScanController()
          : this(new ScanDriverFactory(), new RemotePostProcessor())
        {
        }

        public RemoteScanController(IScanDriverFactory scanDriverFactory, IRemotePostProcessor remotePostProcessor)
        {
            this.scanDriverFactory = scanDriverFactory;
            this.remotePostProcessor = remotePostProcessor;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options)
        {
            var deviceList = scanDriverFactory.Create(options).GetDeviceList(options);
            if (options.Driver == Driver.Twain && !options.TwainOptions.IncludeWiaDevices)
            {
                deviceList = deviceList.Where(x => !x.ID.StartsWith("WIA-", StringComparison.InvariantCulture)).ToList();
            }
            return deviceList;
        }

        public async Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback)
        {
            var driver = scanDriverFactory.Create(options);
            await driver.Scan(options, progress, cancelToken, image =>
            {
                var (scannedImage, postProcessingContext) = remotePostProcessor.PostProcess(image);
                callback(scannedImage, postProcessingContext);
            });
        }
    }
}
