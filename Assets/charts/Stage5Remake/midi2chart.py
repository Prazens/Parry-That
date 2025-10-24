# -*- coding:utf-8 -*-
import mido
import json
import glob

# 자신의 미디 파일에서 목표로 하는 노트의 음정과 변수명을 입력
# 변수명은 안쓰이고 사람이 구별할 때만 쓰이니 맘대로 지어도 됨
const_note_pitch = {"appear": 1, "disappear": 3,
                    "gt_weak_ready": 42, "gt_strong_ready": 44, "gt_weak_hit": 45, "gt_strong_hit": 47,
                    "gt_hold_start_ready": 38, "gt_hold_finish_ready": 36,
                    "gt_hold_start_hit": 37,   "gt_hold_finish_hit": 35}
# 41,40,39: 연타, 38,37,36,35: 홀드
# 반음 => 1, 밑에 로그 뜨는 것 이용해서 그냥 돌리고 첫번째 로그의 note값 확인해서 알아낼 수 있음

# 음정값 : (공격인지 아닌지 bool, Attacktype)
pitch_to_type = {1: (False, -1), 3: (False, -2),
                 42: (False, 0), 44: (False, 1), 45: (True, 0), 47: (True, 1),
                 38: (False, 2), 36: (False, 3), 37: (True, 2), 35: (True, 3)}

# 미디 구조
# "note_type" = "note_on" 또는 "note_off"값을 가짐 : 음의 시작, 끝을 표시
# "note" = int값 : 음의 높낮이 (반음 단위)를 표시 => 강공격/약공격/홀드 등을 구분하는 값으로 사용
# "time" = int값 : 이전 기록 (note_on/off 모두 포함) 에서부터 지난 틱 수
#                  msg.time / midi.ticks_per_beat <== 이런 식으로 사용


def midi_to_json(midi_file_path):
    # midi_file_path 에 변환할 MIDI 파일 주소 입력
    
    # MIDI 파일 열기
    midi = mido.MidiFile(midi_file_path)

    # 변환할 데이터 저장
    midi_datas = []

    # 마지막 유효 노트의 시작으로부터 지난 박자
    last_time = 0

    # 마지막으로 타격 시간이 주어진 노트의 인덱스
    last_hit = -1

    # 각 트랙을 JSON 형태로 변환
    for track in midi.tracks:
        for msg in track:

            if is_print_log:
                # 로그 출력
                print(msg)

            if msg.type == "control_change":
                continue

            if msg.type == 'set_tempo':
                # tempo는 마이크로초로 주어짐, 이를 BPM으로 변환
                bpm = mido.tempo2bpm(msg.tempo)

            elif not msg.is_meta:  # 메타 이벤트 제외

                # 
                if msg.type == "note_on" and msg.note in const_note_pitch.values():
                    # print(midi.ticks_per_beat)
                    
                    # 시작으로부터 지난 박자 수 계산
                    last_time += msg.time / midi.ticks_per_beat
                    print(f"time = {last_time}")

                    if pitch_to_type[msg.note][1] == -1:
                        midi_datas.append(dict())

                        midi_datas[-1]["bpm"] = bpm
                        midi_datas[-1]["strikerType"] = strikerType
                        midi_datas[-1]["direction"] = direction
                        midi_datas[-1]["appearTime"] = last_time
                        midi_datas[-1]["disappearTime"] = -1
                        midi_datas[-1]["notes"] = []

                        last_hit = -1

                    elif pitch_to_type[msg.note][1] == -2:
                        midi_datas[-1]["disappearTime"] = last_time

                    elif not pitch_to_type[msg.note][0]:
                        if pitch_to_type[msg.note][1] >= 2:
                            if len(midi_datas[-1]["notes"]) == 0 or midi_datas[-1]["notes"][-1]["type"] < 2 or\
                               midi_datas[-1]["notes"][-1]["arriveTime"] != -1:

                                midi_datas[-1]["notes"].append({
                                    "time": last_time,
                                    "arriveTime": -1,
                                    "type": pitch_to_type[msg.note][1]
                                })

                        else:
                            # 데이터 추가
                            midi_datas[-1]["notes"].append({
                                "time": last_time,
                                "arriveTime": -1,
                                "type": pitch_to_type[msg.note][1]
                            })

                    elif pitch_to_type[msg.note][0]:
                        last_hit += 1
                        # if len(midi_datas[-1]["notes"]) == 0:
                        #     midi_datas[-2]["notes"][-1]["arriveTime"] = last_time;
                        midi_datas[-1]["notes"][last_hit]["arriveTime"] = last_time

                # 노트가 필요없는 노트
                # 시간 증가값 대입
                else:
                    last_time += msg.time / midi.ticks_per_beat
                    print(f"time = {last_time}")

    # JSON 파일로 저장
    # 이미 그 자리에 파일이 있을 경우 덮어쓰기, 없을 경우 새로 생성
    if len(midi_datas) == 1:
        t_path = f"{midi_file_path[:-4]}.json"
        with open(t_path, "w", encoding="utf-8") as f:
            json.dump(midi_datas[0], f, indent=4)
    else:
        for i in range(len(midi_datas)):
            t_path = f"{midi_file_path[:-4]}_{i + 1}.json"
            with open(t_path, "w", encoding="utf-8") as f:
                json.dump(midi_datas[i], f, indent=4)


if input("로그 출력? (y/n) >>> ") in ["y", "Y"]:
    is_print_log = True
    print("로그 출력")
else:
    is_print_log = False
    print("로그 미출력")

# 사용
for midi_file in glob.glob(f"*.mid", recursive=False) + glob.glob(f"*.midi", recursive=False):
    print(f"\n현재 파일 : {midi_file}")

    if not input("변환? (y/n) >>> ") in ["y", "Y"]:
        print("미변환")
        continue

    strikerType = int(input("strikerType을 입력 >>> "))
    direction = int(input("direction을 입력 >>> "))

    print(f"\n{midi_file} 변환 시도")
    midi_to_json(midi_file)
    print(f"\n{midi_file} 변환 성공")

print("프로그램 종료\n")