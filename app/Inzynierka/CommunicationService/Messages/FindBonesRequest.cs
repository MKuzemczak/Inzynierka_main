﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class FindBonesRequest : BaseMessage
    {
        public List<string> ImagePaths { get; set; }

        public FindBonesRequest(string sender, string receiver, List<string> imagePaths)
        {
            Sender = sender;
            Receiver = receiver;
            ImagePaths = imagePaths;
        }

        public override string ToJson()
        {
            return PrepareJson(contents: JsonSerializer.Serialize(ImagePaths));
        }
    }
}