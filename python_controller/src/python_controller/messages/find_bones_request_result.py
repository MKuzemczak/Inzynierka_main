from . import BaseMessage
from python_controller.structures import ImageBoneSearchResults, ImageBoneSearchResultsList

class FindBonesRequestResult(BaseMessage):
    def __init__(
        self,
        sender: str,
        receiver: str,
        request_id: int,
        image_bone_search_results: ImageBoneSearchResultsList = []
    ):
        self._sender = sender
        self._receiver = receiver
        self._request_id = request_id
        self.image_bone_search_results = image_bone_search_results

    def __str__(self):
        return "{\"image_bone_search_results\": [" + ", ".join([str(result) for result in self.image_bone_search_results]) + "]}"
    
    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    @property
    def request_id(self) -> int:
        return self._request_id

    def to_json(self):
        return self._prepare_json([results.dict() for results in self.image_bone_search_results])

    @classmethod
    def get_instance(cls, sender: str, receiver: str, request_id: int, contents: list):
        return FindBonesRequestResult(
            sender,
            receiver,
            request_id,
            [ImageBoneSearchResults(**data_dict) for data_dict in contents]
        )
