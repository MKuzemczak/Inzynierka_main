from python_controller.messages import *
from python_controller.structures import BoneSearchResult
from python_controller.services import RabbitMQCommunicationService

s = RabbitMQCommunicationService()

def callback(message):
    exit()

s.subscribe(ExitRequest.__name__, callback)
s.start()



# json = "{\"name\":\"FindBonesRequest\",\"contents\":[{}]}"
# "{"name":"FindBonesRequest","contents":[{"hello": "world"}]}"
# m = message_from_json(json)

# print(m)

# d = {"x": 1, "w": 3, "y": 2, "h": 4, "confidence": 5, "detected_class_name": "hehe"}

# b = BoneSearchResult(**d)

# print(b.__dict__)