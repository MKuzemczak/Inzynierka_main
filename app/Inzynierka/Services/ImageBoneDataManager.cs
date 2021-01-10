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
    public class ImageBoneDataManager
    {
        class RequestData
        {
            public List<ImageItem> ImageItems;
            public List<ImageBoneData> Results { get; set; }
            public Action<List<ImageItem>, List<ImageBoneData>> Callback;

            public RequestData(List<ImageItem> imageItem, Action<List<ImageItem>, List<ImageBoneData>> callback)
            {
                ImageItems = imageItem;
                Callback = callback;
                Results = new List<ImageBoneData>();
            }
        };

        private Dictionary<int, RequestData> requestIdToRequestData = new Dictionary<int, RequestData>();
        private int requestIdCntr = 0;

        public ImageBoneDataManager()
        {
            RabbitMQCommunicationService.Instance.Subscribe(typeof(FindBonesRequestResult).Name, FindBonesRequestResultCallback);
        }

        public async Task SaveBoneDataAsync(ImageItem imageItem, ImageBoneData data)
        {
            var boneDataFile = await GetBoneDataFileAsync(imageItem);

            if (boneDataFile is null)
                boneDataFile = await CreateBoneDataFileAsync(imageItem);

            await FileIO.WriteTextAsync(boneDataFile, JsonSerializer.Serialize(data));
        }

        public async Task FindBonesAsync(List<ImageItem> imageItems, Action<List<ImageItem>, List<ImageBoneData>> callback)
        {
            var results = new List<ImageBoneData>();

            var imagesWithNoBoneDataFileAvailable = new List<ImageItem>();

            foreach (var imgItem in imageItems)
            {
                var boneDataFile = await GetBoneDataFileAsync(imgItem);

                if (boneDataFile is object)
                {
                    results.Add(await ReadBoneDataFileAsync(boneDataFile));
                    continue;
                }

                imagesWithNoBoneDataFileAvailable.Add(imgItem);
            }

            if (imagesWithNoBoneDataFileAvailable.Count == 0)
            {
                callback(imageItems, results);
                return;
            }

            StateMessagingService.Instance.SendLoadingMessage("Searching for bones...", 2000);

            requestIdToRequestData.Add(requestIdCntr, new RequestData(imageItems, callback));

            RabbitMQCommunicationService.Instance.Send(new FindBonesRequest(
                RabbitMQCommunicationService.IncomingQueueName,
                RabbitMQCommunicationService.PythonQueueName,
                requestIdCntr,
                imagesWithNoBoneDataFileAvailable.Select(i => i.FilePath).ToList()));

            requestIdCntr++;
        }

        private string GetBoneDataFileName(ImageItem imageItem)
        {
            return imageItem.Filename.Split(".")[0] + ".bdf";
        }

        private async Task<StorageFile> GetBoneDataFileAsync(ImageItem imageItem)
        {
            var parentFolder = await imageItem.File.GetParentAsync();
            var file = await parentFolder.TryGetItemAsync(GetBoneDataFileName(imageItem));

            if (file is object)
                return file as StorageFile;

            return null;
        }

        private async Task<StorageFile> CreateBoneDataFileAsync(ImageItem imageItem)
        {
            var parentFolder = await imageItem.File.GetParentAsync();
            return await parentFolder.CreateFileAsync(GetBoneDataFileName(imageItem));
        }


        private async Task<ImageBoneData> ReadBoneDataFileAsync(StorageFile file)
        {
            string json = await FileIO.ReadTextAsync(file);

            return JsonSerializer.Deserialize<ImageBoneData>(json);
        }

        private async void FindBonesRequestResultCallback(BaseIndication message)
        {
            await MainThreadDispatcherService.MarshalAsyncMethodToMainThreadAsync(async () =>
            {
                var msg = message as FindBonesRequestResult;
                var imgItems = requestIdToRequestData[msg.MessageId].ImageItems;
                var requestDataResults = requestIdToRequestData[msg.MessageId].Results;
                var callback = requestIdToRequestData[msg.MessageId].Callback;
                var messageResults = msg.Contents;

                if (msg.Status == ResultStatus.Failed)
                {
                    StateMessagingService.Instance.SendInfoMessage("Bone search failed", 2000);
                    callback(imgItems, requestDataResults);
                    return;
                }

                StateMessagingService.Instance.SendInfoMessage("Bone search done", 2000);

                

                foreach (var result in messageResults)
                {
                    var imgItem = imgItems.Find(i => { return result.ImagePath == i.FilePath; });

                    requestDataResults.Add(result);
                    await SaveBoneDataAsync(imgItem, result);
                }

                requestIdToRequestData.Remove(msg.MessageId);
                callback(imgItems, requestDataResults);
            });
        }
    }
}
