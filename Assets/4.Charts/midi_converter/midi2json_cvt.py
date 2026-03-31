import mido, json


def dev_print(arg):
    if converter_arg == "d":
        print(arg)


# 변환 표 불러오기

try:
    with open("convert_table.json", "r", encoding="utf-8") as f:
        convert_table = json.load(f)
except Exception as e:
    print(f"convert_table.json 파일에 읽는 중 문제가 발생했습니다\n{e}")

# Read only
direction_map = convert_table["direction_map"]
type_map = convert_table["type_map"]
signal_map = convert_table["signal_map"]
pitch_map = {int(k): {"direction": direction_map[v["direction"]], "type": type_map[v["type"]],
                      "signal": signal_map[v["signal"]]}
             for k, v in convert_table["pitch_map"].items()}
# ex) {12: {"direction": 0, "type": 0}, ...}

while True:

    # 미디 불러오기

    midi_filename = input("파일 이름을 입력 >>> ").split(".")[0]

    try:
        midi_file = mido.MidiFile(midi_filename + ".mid")
    except Exception as e:
        print(f"{midi_filename + ".mid"} 미디 파일을 찾을 수 없습니다\n{e}")
        exit()

    # 정보 입력

    bpm = int(input("bpm 입력 >>> "))

    # 디버그용
    if bpm == -1:
        for msg in midi_file.tracks[0]:
            print(msg)
        exit()

    beatmap = {"bpm": bpm, "strikers": [], "notes": []}

    for i in direction_map.keys():
        striker_index = input(f"direction {i} 에 대한 스트라이커 인덱스 입력 (입력 없을 시 스킵)\n>>> ")
        if striker_index == "0" or striker_index == "1" or striker_index == "2" or striker_index == "3":
            appear_time = float(input("스트라이커 등장 박자 입력 >>> "))
            disappear_time = float(input("스트라이커 퇴장 박자 입력 >>> "))
            beatmap["strikers"].append(
                {
                    "strikerType": int(striker_index),
                    "direction": direction_map[i],
                    "appearTime": appear_time,
                    "disappearTime": disappear_time
                }
            )

    converter_arg = input("엔터 키로 변환 시작 >>> ")

    # 변환

    dev_print("#######################################")
    # tempo = 500000  # default: 120 bpm
    ticks_per_beat = midi_file.ticks_per_beat
    dev_print(f"ticks_per_beat set to {ticks_per_beat}tick/beat")
    cur_tick = 0
    cur_hit_index = 0

    for msg in midi_file.tracks[0]:
        dev_print(msg)
        cur_tick += msg.time

        # if msg.type == 'set_tempo':
        #     tempo = msg.tempo
        #     dev_print(f"tempo set to {tempo}ys/beat")

        if msg.type == 'note_on':
            note_info = pitch_map[msg.note]
            cur_beat = cur_tick / ticks_per_beat
            dev_print(f"\nAt {cur_beat} beat(s):")

            # 신호인 경우
            if note_info["signal"] == 0:

                # 노트 표기 부분에 스트라이커 인덱스 표기
                # 나중에 스트라이커 인덱스 대신 방향 표기로 바뀔 수 있음
                tmp_striker_index = None
                for i in range(len(beatmap["strikers"])):
                    if note_info["direction"] == beatmap["strikers"][i]["direction"]:
                        tmp_striker_index = i

                beatmap["notes"].append(
                    {
                        "strikerIndex": tmp_striker_index,
                        "noticeBeat": cur_beat,
                        "arriveBeat": -1,
                        "type": note_info["type"]
                    }
                )
                dev_print(beatmap["notes"][-1])

            # 공격인 경우
            elif note_info["signal"] == 1:
                beatmap["notes"][cur_hit_index]["arriveBeat"] = cur_beat
                dev_print(beatmap["notes"][cur_hit_index])
                cur_hit_index += 1

    # json 파일로 내보내기

    dev_print(f"{midi_filename}.json 에 내보내기 시도")
    with open(f"{midi_filename}.json", "w", encoding="utf-8") as f:
        json.dump(beatmap, f, ensure_ascii=False, indent=2)

    print(f"{midi_filename}.json 으로 내보냈습니다\n")
