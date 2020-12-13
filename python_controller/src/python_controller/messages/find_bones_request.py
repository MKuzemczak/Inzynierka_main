import json

from . import BaseMessage

class FindBonesRequest(BaseMessage):

    def __init__(self, sender: str, receiver: str, request_id: int, img_paths: list = []):
        self._sender = sender
        self._receiver = receiver
        self._request_id = request_id
        self.image_paths = img_paths
    
    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    def to_json(self):
        return self._prepare_json(self.image_paths)

    @property
    def request_id(self) -> int:
        return self._request_id

    @classmethod
    def get_instance(cls, sender: str, receiver: str, request_id: int, contents: list):
        return FindBonesRequest(sender, receiver, request_id, contents)
