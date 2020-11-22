from python_controller.messages import FindBonesRequestMessage, FindBonesRequestResultMessage
from python_controller.structures import BoneSearchResult

json = "{\"name\": \"FindBonesRequestResult\", \"contents\": [{\"x\":1,\"y\":2}]}"
m = FindBonesRequestResultMessage.from_json(json)
print(m)

# d = {"x": 1, "w": 3, "y": 2, "h": 4, "confidence": 5, "detected_class_name": "hehe"}

# b = BoneSearchResult(**d)

# print(b.__dict__)