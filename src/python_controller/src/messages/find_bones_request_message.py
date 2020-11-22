import json

from . import BaseMessage

class FindBonesRequestMessage(BaseMessage):
    _name = "FindBonesRequest"

    def __init__(self, img_paths: str = ""):
        self.image_paths = img_paths
    
    def name(self):
        return self._name
    
    def to_json(self):
        return self._prepare_json(self.image_paths)

    @classmethod
    def get_instance_from_message_contents(cls, contents: list):


        return FindBonesRequestMessage(contents)