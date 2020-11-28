from . import BaseMessage
from python_controller.structures import BoneSearchResult, BoneSearchResultList

class FindBonesRequestResult(BaseMessage):
    def __init__(self, bone_search_results: BoneSearchResultList = []):
        self.bone_search_results = bone_search_results

    def __str__(self):
        return "{\"bone_search_results\": [" + ", ".join([str(result) for result in self.bone_search_results]) + "]}"
    
    def to_json(self):
        return self._prepare_json([result.__dict__ for result in self.bone_search_results])

    @classmethod
    def get_instance_from_message_contents(self, contents: list):
        return FindBonesRequestResult([BoneSearchResult(**data_dict) for data_dict in contents])
