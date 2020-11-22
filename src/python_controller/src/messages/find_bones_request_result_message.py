from . import BaseMessage
from ..structures import BoneSearchResult

class FindBonesRequestResultMessage(BaseMessage):
    _name = "FindBonesRequestResult"

    def __init__(self, bone_search_results: list = []):
        self.bone_search_results = bone_search_results

    def __str__(self):
        return "{\"bone_search_results\": [" + ", ".join([str(result) for result in self.bone_search_results]) + "]}"

    @property
    def name(self):
        return self._name
    
    def to_json(self):
        return self._prepare_json([result.__dict__ for result in self.bone_search_results])

    @classmethod
    def get_instance_from_message_contents(self, contents: list):
        return FindBonesRequestResultMessage([BoneSearchResult(**data_dict) for data_dict in contents])
