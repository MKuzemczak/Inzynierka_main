import json

from . import BaseRequest

class FindBonesRequest(BaseRequest):

    def __init__(self, sender: str, receiver: str, message_id: int, contents: list = []):
        self._sender = sender
        self._receiver = receiver
        self._message_id = message_id
        self.image_paths = contents
    
    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    def to_json(self):
        return self._prepare_json(self.image_paths)

    @property
    def message_id(self) -> int:
        return self._message_id

    @classmethod
    def get_instance(cls, sender: str, receiver: str, message_id: int, contents: list):
        return FindBonesRequest(sender, receiver, message_id, contents)
