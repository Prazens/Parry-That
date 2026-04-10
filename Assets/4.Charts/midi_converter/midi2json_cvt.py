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

    beatmap = {"bpm": float(input("bpm 입력 >>> ")),
               "strikerType": int(input(f"스트라이커 인덱스 입력 >>> ")),
               "startBeat": float(input("스트라이커 등장 박자 입력 >>> ")),
               "endBeat": float(input("스트라이커 퇴장 박자 입력 >>> ")),
               "notes": []}

    # 디버그용
    if beatmap["bpm"] == -1:
        for msg in midi_file.tracks[0]:
            print(msg)
        exit()

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

                beatmap["notes"].append(
                    {
                        "direction": note_info["direction"],
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

    if beatmap["notes"][0]["noticeBeat"] < beatmap["startBeat"]:
        print(f"경고: 등장 박자 {beatmap["startBeat"]}가 첫 노트의 noticeBeat {beatmap["notes"][0]["noticeBeat"]}보다 늦습니다")

    if beatmap["notes"][-1]["arriveBeat"] > beatmap["endBeat"]:
        print(f"경고: 퇴장 박자 {beatmap["endBeat"]}가 마지막 노트의 arriveBeat {beatmap["notes"][-1]["arriveBeat"]}보다 빠릅니다")

    # json 파일로 내보내기

    dev_print(f"{midi_filename}.json 에 내보내기 시도")
    with open(f"{midi_filename}.json", "w", encoding="utf-8") as f:
        json.dump(beatmap, f, ensure_ascii=False, indent=2)

    print(f"{midi_filename}.json 으로 내보냈습니다\n")
