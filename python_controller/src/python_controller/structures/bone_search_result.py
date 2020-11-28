from collections import namedtuple

class BoneSearchResult:
    def __init__(self, x: int, y: int, w: int, h: int, confidence: float, detected_class_name: str):
        
        self.x = x
        self.y = y
        self.w = w
        self.h = h
        self.confidence = confidence
        self.detected_class_name = detected_class_name

    def __str__(self):
        return str(self.__dict__)

BoneSearchResultList = list[BoneSearchResult]
