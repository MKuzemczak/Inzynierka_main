from . import BaseMessage
from python_controller.structures import BoneSearchResult, BoneSearchResultList

class FindBonesRequestResult(BaseMessage):
    def __init__(self, sender: str, receiver: str, bone_search_results: BoneSearchResultList = []):
        self._sender = sender
        self._receiver = receiver
        self.bone_search_results = bone_search_results

    def __str__(self):
        return "{\"bone_search_results\": [" + ", ".join([str(result) for result in self.bone_search_results]) + "]}"
    
    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver


    def to_json(self):
        return self._prepare_json([str(result) for result in self.bone_search_results])

    @classmethod
    def get_instance(cls, sender: str, receiver: str, contents: list):
        return FindBonesRequestResult(sender, receiver, [BoneSearchResult(**data_dict) for data_dict in contents])
