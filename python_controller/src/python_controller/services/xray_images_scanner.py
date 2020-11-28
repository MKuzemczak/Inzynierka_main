from python_controller.messages import FindBonesRequest, FindBonesRequestResult
from python_controller.structures import BoneSearchResult, BoneSearchResultList

class XRayImagesScanner:
    def __init__(self, communication_service):
        self.communication_service = communication_service

        self.communication_service.subscribe(FindBonesRequest, self._find_bones_request_callback)

    def _find_bones_request_callback(self, request_message):
        results = self._find_bones(request_message.image_paths)
        result_message = FindBonesRequestResult(results)

        self.communication_service.publish(result_message)

    def _find_bones(self, image_paths: list = []) -> BoneSearchResultList:
        return [BoneSearchResult(1, 2, 3, 4, 5, "ulna")]
