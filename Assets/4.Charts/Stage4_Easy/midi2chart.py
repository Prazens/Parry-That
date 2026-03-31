# -*- coding:utf-8 -*-
import glob
import json

# 폴더 내 모든 JSON 파일 처리
for filename in glob.glob(f"*.json", recursive=False):
    with open(filename, "r", encoding="utf-8") as file:
        data = json.load(file)
        data["appearTime"] -= 4
        data["disappearTime"] -= 4

    # notes의 time과 arriveTime을 4 감소
    for note in data.get("notes", []):
        note["time"] -= 4
        note["arriveTime"] -= 4

    # 수정된 데이터를 다시 JSON 파일에 저장
    with open(filename, "w", encoding="utf-8") as file:
        json.dump(data, file, indent=4, ensure_ascii=False)

    print(f"Updated: {filename}")
