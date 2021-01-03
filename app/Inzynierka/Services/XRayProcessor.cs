using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Windows.Storage;

using Inzynierka.CommunicationService;
using Inzynierka.CommunicationService.Messages;
using Inzynierka.MainThreadDispatcher;
using Inzynierka.Models;
using Inzynierka.StateMessaging;

namespace Inzynierka.Services
{
    public class XRayProcessor
    {
        class RequestData
        {
            public ImageItem ImageItem;
            public Action<ImageItem, List<ImageBoneSearchResults>> Callback;

            public RequestData(ImageItem imageItem, Action<ImageItem, List<ImageBoneSearchResults>> callback)
            {
                ImageItem = imageItem;
                Callback = callback;
            }
        };

        private Dictionary<int, RequestData> requestIdToRequestData = new Dictionary<int, RequestData>();
        private int requestIdCntr = 0;

        public XRayProcessor()
        {
            RabbitMQCommunicationService.Instance.Subscribe(typeof(FindBonesRequestResult).Name, FindBonesRequestResultCallback);
        }

        public async Task FindBonesAsync(ImageItem imgItem, Action<ImageItem, List<ImageBoneSearchResults>> callback)
        {
            var parentFolder = await imgItem.File.GetParentAsync();
            var boneDataFile = await parentFolder.TryGetItemAsync(imgItem.Filename.Split(".")[0] + ".bdf");

            if (boneDataFile is object)
            {
                var results = await ReadBoneDataFileAsync(boneDataFile as StorageFile);

                callback(imgItem, results);

                return;
            }

            StateMessagingService.Instance.SendLoadingMessage("Searching for bones...", 2000);

            requestIdToRequestData.Add(requestIdCntr, new RequestData(imgItem, callback));

            RabbitMQCommunicationService.Instance.Send(new FindBonesRequest(
                RabbitMQCommunicationService.IncomingQueueName,
                RabbitMQCommunicationService.PythonQueueName,
                requestIdCntr,
                new List<string>() { imgItem.FilePath }));

            requestIdCntr++;
        }

        private async Task<List<ImageBoneSearchResults>> ReadBoneDataFileAsync(StorageFile file)
        {
            var result = new List<ImageBoneSearchResults>();

            string json = await FileIO.ReadTextAsync(file);

            return JsonSerializer.Deserialize<List<ImageBoneSearchResults>>(json);
        }

        private async void FindBonesRequestResultCallback(BaseIndication message)
        {
            await MainThreadDispatcherService.MarshalAsyncMethodToMainThreadAsync(async () =>
            {
                StateMessagingService.Instance.SendInfoMessage("Bone search done", 2000);

                var msg = message as FindBonesRequestResult;
                var imgItem = requestIdToRequestData[msg.MessageId].ImageItem;
                var callback = requestIdToRequestData[msg.MessageId].Callback;
                var results = msg.Contents;
                var parentFolder = await imgItem.File.GetParentAsync();
                var boneDataFile = await parentFolder.CreateFileAsync(imgItem.Filename.Split(".")[0] + ".bdf");

                await FileIO.WriteTextAsync(boneDataFile, JsonSerializer.Serialize(results));

                callback(imgItem, msg.Contents);
            });
        }
    }
}
