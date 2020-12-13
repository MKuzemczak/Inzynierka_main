from . import BoneSearchResultList

class ImageBoneSearchResults:
    def __init__(self, image_path: str, bone_search_results: BoneSearchResultList):
        self.image_path = image_path
        self.bone_search_results = bone_search_results
    
    def __str__(self):
        return str({
            "image_path": self.image_path,
            "bone_search_results": [result.__dict__ for result in self.bone_search_results]
        })

    def dict(self):
        return {
            "image_path": self.image_path,
            "bone_search_results": [result.__dict__ for result in self.bone_search_results]
        }

ImageBoneSearchResultsList = list[ImageBoneSearchResults]
