import json

while True:

    # 불러오기

    orig_filename = input("자르고자 하는 파일 이름을 입력 >>> ").split(".")[0]

    with open(f"{orig_filename}.json", "r", encoding="utf-8") as f:
        orig_chart = json.load(f)

    cut_info = []
    cut_num = int(input("몇 개의 파일로 자르고자 하는지 입력 >>> "))
    for i in range(cut_num):
        file_name = input(f"{i + 1}번째의 파일 이름을 입력 (비워놓을 시 자동 입력) >>> ").split(".")[0]
        striker_type = input("스트라이커 인덱스 입력 (비워놓을 시 원본 파일에서 가져옴) >>> ")
        cut_info.append(
            {
                "fileName": f"{orig_filename}_{i + 1}" if file_name == "" else file_name,
                "strikerType": orig_chart["strikerType"] if file_name == "" else int(striker_type),
                "startBeat": float(input("스트라이커 등장 박자 입력 >>> ")),
                "endBeat": float(input("스트라이커 퇴장 박자 입력 >>> ")),
                "cutBeat": 99999 if i + 1 == cut_num else float(input("(arriveBeat 기준) 자를 박자 입력 (포함) >>> "))
            }
        )

    cut_info = sorted(cut_info, key=lambda x: x["cutBeat"])

    # 채보 자르기

    bpm = orig_chart["bpm"]
    cut_chart = [{"bpm": bpm,
                  "strikerType": chart_info["strikerType"],
                  "startBeat": chart_info["startBeat"],
                  "endBeat": chart_info["endBeat"],
                  "notes": []}
                 for chart_info in cut_info]
    cut_index = 0

    for note in orig_chart["notes"]:
        if note["arriveBeat"] > cut_info[cut_index]["cutBeat"]:
            cut_index += 1

        cut_chart[cut_index]["notes"].append(note)

    # 내보내기

    for i in range(cut_num):
        with open(f"{cut_info[i]["fileName"]}.json", "w", encoding="utf-8") as f:
            json.dump(cut_chart[i], f, ensure_ascii=False, indent=2)
        print(f"{cut_info[i]["fileName"]}.json 으로 내보냈습니다\n")
