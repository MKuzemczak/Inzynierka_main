import json

from . import BaseMessage

class FindBonesRequest(BaseMessage):

    def __init__(self, sender: str, receiver: str, img_paths: list = []):
        self._sender = sender
        self._receiver = receiver
        self.image_paths = img_paths
    
    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    def to_json(self):
        return self._prepare_json(self.image_paths)

    @classmethod
    def get_instance(cls, sender: str, receiver: str, contents: list):
        return FindBonesRequest(sender, receiver, contents)
