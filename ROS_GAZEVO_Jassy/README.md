# ROS2 Jazzy 로봇 개발: URDF, Gazebo, SLAM, Nav2

> **가제보 Jazzy 159 교재 — Chapter 01 & 02**

---

## 목차

### Chapter 01: ROS2 개발환경 구축과 URDF 로봇 모델링

* 이 장에서는 ROS2 개발 환경을 구축한 후, urdf 파일을 작성하여 로봇을 정의해 봅니다.
*  학습 목표
   * 이 장을 완료하면 학습자는 다음을 할 수 있습니다.
   * ROS2 개발 환경을 구축할 수 있다.
   * 로봇의 구조를 표현하는 기본 모델링 방법을 이해할 수 있다. ∙로봇의 좌표계와 프레임 개념을 익힐 수 있다.
   * RViz2에서 로봇 모델을 시각적으로 확인할 수 있다.

| 절 | 제목 | 파일 |
|---|---|---|
| 01 | [ROS2의 이해](CH01/01_ROS2의_이해.md) | `CH01/01_ROS2의_이해.md` |
| 02 | [ROS2 개발 환경 구성하기](CH01/02_ROS2_개발환경_구성.md) | `CH01/02_ROS2_개발환경_구성.md` |
| 03 | [URDF의 이해](CH01/03_URDF의_이해.md) | `CH01/03_URDF의_이해.md` |
| 04 | [URDF로 로봇 구조 정의하기](CH01/04_URDF로_로봇_구조_정의.md) | `CH01/04_URDF로_로봇_구조_정의.md` |
| 04-01 | [04_1_myros_sketch_폴더_및_기본_파일_생성하기.md](04_1_myros_sketch_폴더_및_기본_파일_생성하기.md) | `CH01/04_1_myros_sketch_폴더_및_기본_파일_생성하기.md` |
| 04-02 | [04_2_base_link_정의하기.md](04_2_base_link_정의하기.md) | `CH01/04_2_base_link_정의하기.md` |
| 04-03 | [04_3_chassis_joint_chassis_추가하기.md](04_3_chassis_joint_chassis_추가하기.md) | `CH01/04_3_chassis_joint_chassis_추가하기.md` |
| 04-04 | [04_4_구동축_및_바퀴_추가하기.md](04_4_구동축_및_바퀴_추가하기.md) |` |
| 04-05 | [04_5_캐스터_추가하기.md](04_5_캐스터_추가하기.md) |` |
| 04-06 | [04_6_esp32s3_joint_esp32s3_frame_추가하기.md](04_6_esp32s3_joint_esp32s3_frame_추가하기.md) |` |
| 04-07 | [04_7_imu_joint_imu_link_추가하기.md](04_7_imu_joint_imu_link_추가하기.md) |` |
| 04-08 | [04_8_rpi_base_joint_rpi_base_추가하기.md](04_8_rpi_base_joint_rpi_base_추가하기.md) |` |
| 04-09 | [04_9_rpi_joint_rpi_frame_추가하기.md](04_9_rpi_joint_rpi_frame_추가하기.md) | ` |
| 04-10 | [04_10_lidar_base_joint_lidar_base_frame_추가하기.md](04_10_lidar_base_joint_lidar_base_frame_추가하기.md) |` |
| 04-11 | [04_11_laser_joint_laser_frame_추가하기.md](04_11_laser_joint_laser_frame_추가하기.md) |` |
| 04-12 | [04_12_camera_joint_camera_link_추가하기.md](04_12_camera_joint_camera_link_추가하기.md) |` |
| 04-13 | [04_13_camera_optical_joint_camera_link_optical_추가하기.md](04_13_camera_optical_joint_camera_link_optical_추가하기.md) |` |
| 04-14 | [04_14_base_footprint_joint_base_footprint_추가하기.md](04_14_base_footprint_joint_base_footprint_추가하기.md) |` |
| 04-15 | [04_15_collision_태그_추가하기.md](04_15_collision_태그_추가하기.md) |` |
| 04-16 | [04_16_inertial_태그_추가하기.md](04_16_inertial_태그_추가하기.md) |` |

### Chapter 02: 가제보 로봇 구동과 SLAM·Nav2 자율주행

* 이 장에서는 앞서 작성한 URDF 로봇 모델을 가제보(Gazebo)에 실제로  생성하여 구동해  보고, 라이다·카메라 등 센서 데이터를 확인합니다.
* 이어서 워크스페이스와  패키지를 구성하고 launch 파일을 작성해 로봇 구동 과정을 자동화한 뒤, SLAM으로 맵을 작성하고 Nav2를 활 용한 자율주행까지 실습해 봅니다.

*  학습 목표
   * 이 장을 완료하면 학습자는 다음을 할 수 있습니다.
   * 가제보에 로봇을 생성하고 구동할 수 있다.
   * /cmd_vel 토픽과 teleop_twist_keyboard로 로봇을 직접 움직일 수 있다.
   * /joint_states, /odom, /tf를 통해 로봇의 상태를 확인할 수 있다.
   * 라이다 센서를 구동하고 /scan 데이터를 RViz2에서 확인할 수 있다.
   * 카메라 센서를 구동하고 /camera/image_raw 데이터를 RViz2에서 확인할 수 있다.
   * 월드 파일을 활용해 가제보 시뮬레이션 환경을 구성할 수 있다.
   * 워크스페이스와 패키지를 생성하고 launch 파일을 작성해 로봇 구동을 자동화할 수 있다.
   * SLAM Toolbox와 Nav2를 연동해 맵을 작성할 수 있다.
   * Nav2를 활용해 단일 목표점 및 다중 웨이포인트 자율주행을 수행할 수 있다.

* ※ 이 장에서는 로봇을 구동하기 위한 명령들을 ①②③... 순서로 여러 차례 반복해서 실행하 게 됩니다.
* 이 반복은 단순 암기가 아니라, '어떤 명령이 어떤 디렉토리에서 어떤 파일 경로를 인자로 필요로 하는지'를 체화하기 위한 과정입니다.
* 이 이해가 쌓이면 03절에서는 이 명령들 을 launch 파일로 자동화하는 작업을 LLM에게 맡기게 되는데,
* 이때 필요한 것은  Python launch 문법을 암기하는 능력이 아니라 정확한 경로와 명령을 LLM에게 전달할 수 있는 능력 입니다.

| 절 | 제목 | 파일 |
|---|---|---|
| 01 | [가제보에 로봇 생성하고 구동하기](CH02/01_가제보에_로봇_생성하고_구동.md) | `CH02/01_가제보에_로봇_생성하고_구동.md` |
| 02 | [월드 파일 사용해 보기](CH02/02_월드_파일_사용.md) | `CH02/02_월드_파일_사용.md` |
| 03 | [패키지 생성 및 launch 파일 작성](CH02/03_패키지_생성_및_launch_파일.md) | `CH02/03_패키지_생성_및_launch_파일.md` |
| 04 | [SLAM으로 맵 그리기](CH02/04_SLAM으로_맵_그리기.md) | `CH02/04_SLAM으로_맵_그리기.md` |
| 05 | [Nav2로 자율주행 실습하기](CH02/05_Nav2로_자율주행.md) | `CH02/05_Nav2로_자율주행.md` |

---

## 이미지 안내

- 본 교재의 이미지(`image1.bmp` ~ `image265.bmp`)는 `img/` 폴더에 보관되어 있습니다.
- 각 마크다운 파일에서는 `![설명](img/imageN.bmp)` 형태로 참조합니다.
- 이미지 번호 매핑은 원본 문서의 삽입 순서를 기준으로 하였습니다.
