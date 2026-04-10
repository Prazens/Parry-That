import json

while True:

    # 채보 입력

    old_chart_files = [input("합치고자 하는 차트를 입력(엔터로 마치기)\n>>> ").split(".")[0]]
    while True:
        tmp = input(">>> ").split(".")[0]
        if tmp == "":
            break
        old_chart_files.append(tmp)

    auto_calc_sb = False
    auto_calc_eb = False

    striker_type = int(input("새로 지정할 스트라이커 타입 입력 >>> "))

    start_beat = float(input("스트라이커 등장 박자 입력(-1이면 자동 계산) >>> "))
    if start_beat == -1:
        auto_calc_sb = True
        start_beat = 99999

    end_beat = float(input("스트라이커 퇴장 박자 입력(-1이면 자동 계산) >>> "))
    if end_beat == -1:
        auto_calc_eb = True

    # 채보 읽기

    old_charts = []
    for chart_name in old_chart_files:
        with open(f"{chart_name}.json", "r", encoding="utf-8") as f:
            old_charts.append(json.load(f))

    # 채보 변환

    bpm = old_charts[0]["bpm"]
    notes = []

    for old_chart in old_charts:

        if auto_calc_sb:
            start_beat = min(start_beat, old_chart["startBeat"])
        if auto_calc_eb:
            end_beat = max(end_beat, old_chart["endBeat"])

        notes += [{"direction": note["direction"], "noticeBeat": note["noticeBeat"],
                   "arriveBeat": note["arriveBeat"], "type": note["type"]}
                  for note in old_chart["notes"]]

    # arriveBeat 기준으로 오름차순 정렬
    notes = sorted(notes, key=lambda x: x["arriveBeat"])

    beatmap = {"bpm": bpm, "strikerType": striker_type, "startBeat": start_beat, "endBeat": end_beat, "notes": notes}

    # 채보 내보내기

    tar_filename = input("내보낼 파일명 입력 >>> ")
    with open(f"{tar_filename}.json", "w", encoding="utf-8") as f:
        json.dump(beatmap, f, ensure_ascii=False, indent=2)
    print(f"{tar_filename}.json 으로 내보냈습니다\n")
