import json

from . import BaseMessage

class FindBonesRequest(BaseMessage):

    def __init__(self, img_paths: str = ""):
        self.image_paths = img_paths
    
    def to_json(self):
        return self._prepare_json(self.image_paths)

    @classmethod
    def get_instance_from_message_contents(cls, contents: list):


        return FindBonesRequest(contents)