import cv2
import numpy as np

from python_controller.messages import FindBonesRequest, FindBonesRequestResult, ResultMessageStatus
from python_controller.structures import BoneSearchResult
from python_controller.structures import BoneSearchResultList
from python_controller.structures import ImageBoneSearchResults
from python_controller.structures import ImageBoneSearchResultsList

class XRayImagesScanner:
    def __init__(
            self,
            communication_service,
            yolo_cfg_file_path: str,
            yolo_weights_file_path: str,
            yolo_names_file_path: str):
        self.communication_service = communication_service

        self.communication_service.subscribe(FindBonesRequest, self._find_bones_request_callback)

        self._yolo_cfg_file_path = yolo_cfg_file_path
        self._yolo_weights_file_path = yolo_weights_file_path
        self._yolo_names_file_path = yolo_names_file_path
        
        self._load_names(self._yolo_names_file_path)

        self._net = cv2.dnn.readNet(self._yolo_weights_file_path, self._yolo_cfg_file_path)

        layer_names = self._net.getLayerNames()
        self._output_layers = [layer_names[i[0] - 1] for i in self._net.getUnconnectedOutLayers()]

        print("setup finished")

    def _find_bones_request_callback(self, request_message):
        results = self._find_bones(request_message.image_paths)
        result_message = FindBonesRequestResult(
            request_message.receiver,
            request_message.sender,
            request_message.request_id,
            ResultMessageStatus.Success,
            results
        )

        self.communication_service.publish(result_message)

    def _load_names(self, yolo_names_file_path: str):
        self._classes = []

        with open(yolo_names_file_path, "r") as f:
            self._classes = [str(line.strip()) for line in f.readlines()]

    
    def _find_bones(self, image_paths: list = []) -> ImageBoneSearchResultsList:
        results = []

        for img_path in image_paths:
            image_results = []

            if img_path.endswith(".png") or img_path.endswith(".PNG"):
                print("scanning for bones: " + img_path)

                img = cv2.imread(img_path)
                height, width, channels = img.shape

                # Detecting objects
                blob = cv2.dnn.blobFromImage(img, 0.00392, (416, 416), (0, 0, 0), True, crop=False)
                self._net.setInput(blob)
                outs = self._net.forward(self._output_layers)

                # Showing informations on the screen
                class_ids = []
                confidences = []
                boxes = []
                for out in outs:
                    for detection in out:
                        scores = detection[5:]
                        class_id = np.argmax(scores)
                        confidence = scores[class_id]
                        if confidence > 0.8:
                            print("Object detected")
                            self._insert_bone_search_result_if_not_exists(image_results, BoneSearchResult(
                                detection[0],
                                detection[1],
                                detection[2],
                                detection[3],
                                float(confidence),
                                self._classes[class_id]))
            
                if len(image_results) > 0:
                    results.append(ImageBoneSearchResults(img_path, image_results))
        
        return results

    def _insert_bone_search_result_if_not_exists(self, result_list: BoneSearchResultList, result: BoneSearchResult):
        location_offset = 0.01
        size_offset = 0.01

        for i in range(len(result_list)):
            pivot = result_list[i]

            if result.detected_class_name != pivot.detected_class_name:
                continue
            
            if (abs(result.x - pivot.x) < location_offset
                    and abs(result.y - pivot.y) < location_offset):
                    # and abs(result.w - pivot.w) < size_offset
                    # and abs(result.h - pivot.h) < size_offset):
                if result.confidence > pivot.confidence:
                    result_list.remove(pivot)
                    break
                
                return

        result_list.append(result)

