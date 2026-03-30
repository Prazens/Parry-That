import json

while True:

    # 채보 입력

    old_chart_files = [input("변환하고자 하는 구 차트를 최대 4개까지 입력(엔터로 구분)\n>>> ").split(".")[0]]
    for i in range(3):
        tmp = input(">>> ").split(".")[0]
        if tmp == "":
            break
        old_chart_files.append(tmp)

    # 채보 읽기

    old_charts = []
    for chart_name in old_chart_files:
        with open(f"{chart_name}.json", "r", encoding="utf-8") as f:
            old_charts.append(json.load(f))

    # direction 기준으로 오름차순 정렬
    old_charts = sorted(old_charts, key=lambda x: x["direction"])

    # 채보 변환
    bpm = old_charts[0]["bpm"]
    strikers = []
    notes = []

    for index, old_chart  in enumerate(old_charts):
        strikers.append(
            {
                "strikerType": old_chart["strikerType"],
                "direction": old_chart["direction"],
                "appearTime": old_chart["appearTime"],
                "disappearTime": old_chart["disappearTime"]
            }
        )
        notes += [{"strikerIndex": index, "noticeBeat": note["time"], "arriveBeat": note["arriveTime"], "type": note["type"]}
                  for note in old_chart["notes"]]

    # arriveBeat 기준으로 오름차순 정렬
    notes = sorted(notes, key=lambda x: x["arriveBeat"])

    beatmap = {"bpm": bpm, "strikers": strikers, "notes": notes}

    # 채보 내보내기

    tar_filename = input("내보낼 파일명 입력 >>> ")
    with open(f"{tar_filename}.json", "w", encoding="utf-8") as f:
        json.dump(beatmap, f, ensure_ascii=False, indent=2)
    print(f"{tar_filename}.json 으로 내보냈습니다\n")
