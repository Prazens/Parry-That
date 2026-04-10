import json

old_chart_filename = input("변환하고자 하는 구 차트를 입력 >>> ").split(".")[0]

with open(f"{old_chart_filename}.json", "r", encoding="utf-8") as f:
    chart = json.load(f)

new_chart = {"bpm": chart["bpm"], "strikerType": chart["strikers"][0]["strikerType"],
             "startBeat": chart["strikers"][0]["appearTime"],
             "endBeat": chart["strikers"][0]["disappearTime"],
             "notes": [{"direction": chart["strikers"][note["strikerIndex"]]["direction"],
                        "noticeBeat": note["noticeBeat"],
                        "arriveBeat": note["arriveBeat"],
                        "type": note["type"]
                        }
                       for note in chart["notes"]]}

new_chart_filename = input("새 이름 입력 >>> ").split(".")[0]

with open(f"{new_chart_filename}.json", "w", encoding="utf-8") as f:
    json.dump(new_chart, f, ensure_ascii=False, indent=2)
print(f"{new_chart_filename}.json 으로 내보냈습니다\n")