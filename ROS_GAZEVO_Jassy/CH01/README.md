# Chapter 01: ROS2 개발환경 구축과 URDF 로봇 모델링

> 이 장에서는 ROS2 개발 환경을 구축한 후, urdf 파일을 작성하여 로봇을 정의해 봅니다.

## 학습 목표

이 장을 완료하면 학습자는 다음을 할 수 있습니다.

- ROS2 개발 환경을 구축할 수 있다.
- 로봇의 구조를 표현하는 기본 모델링 방법을 이해할 수 있다.
- 로봇의 좌표계와 프레임 개념을 익힐 수 있다.
- RViz2에서 로봇 모델을 시각적으로 확인할 수 있다.

## 목차

| 절 | 제목 | 파일 |
|---|---|---|
| 01 | ROS2의 이해 | [01_ROS2의_이해.md](01_ROS2의_이해.md) |
| 02 | ROS2 개발 환경 구성하기 | [02_ROS2_개발환경_구성.md](02_ROS2_개발환경_구성.md) |
| 03 | URDF의 이해 | [03_URDF의_이해.md](03_URDF의_이해.md) |
| 04 | URDF로 로봇 구조 정의하기 | [04_URDF로_로봇_구조_정의.md](04_URDF로_로봇_구조_정의.md) |
| 04-01 | myros_sketch_폴더_및_기본_파일_생성하기 | [04_1_myros_sketch_폴더_및_기본_파일_생성하기.md](04_1_myros_sketch_폴더_및_기본_파일_생성하기.md)|
| 04-02 | base_link_정의하기 |[04_2_base_link_정의하기.md](04_2_base_link_정의하기.md) |
| 04-03 | chassis_joint_chassis_추가하기 | [04_3_chassis_joint_chassis_추가하기.md](04_3_chassis_joint_chassis_추가하기.md) |
| 04-04 | 구동축_및_바퀴_추가하기 | [04_4_구동축_및_바퀴_추가하기.md](04_4_구동축_및_바퀴_추가하기.md) |
| 04-05 | 캐스터_추가하기 | [04_5_캐스터_추가하기.md](04_5_캐스터_추가하기.md) |
| 04-06 | esp32s3_joint_esp32s3_frame_추가하기 | [04_6_esp32s3_joint_esp32s3_frame_추가하기.md](04_6_esp32s3_joint_esp32s3_frame_추가하기.md) |
| 04-07 | imu_joint_imu_link_추가하기 | [04_7_imu_joint_imu_link_추가하기.md](04_7_imu_joint_imu_link_추가하기.md) |
| 04-08 | rpi_base_joint_rpi_base_추가하기 | [04_8_rpi_base_joint_rpi_base_추가하기.md](04_8_rpi_base_joint_rpi_base_추가하기.md) |
| 04-09 | rpi_joint_rpi_frame_추가하기 | [04_9_rpi_joint_rpi_frame_추가하기.md](04_9_rpi_joint_rpi_frame_추가하기.md) | 
| 04-10 | lidar_base_joint_lidar_base_frame_추가하기 | [04_10_lidar_base_joint_lidar_base_frame_추가하기.md](04_10_lidar_base_joint_lidar_base_frame_추가하기.md) |
| 04-11 | laser_joint_laser_frame_추가하기 | [04_11_laser_joint_laser_frame_추가하기.md](04_11_laser_joint_laser_frame_추가하기.md) |
| 04-12 | camera_joint_camera_link_추가하기 | [04_12_camera_joint_camera_link_추가하기.md](04_12_camera_joint_camera_link_추가하기.md) |
| 04-13 | camera_optical_joint_camera_link_optical_추가하기 | [04_13_camera_optical_joint_camera_link_optical_추가하기.md](04_13_camera_optical_joint_camera_link_optical_추가하기.md) |
| 04-14 | base_footprint_joint_base_footprint_추가하기 | [04_14_base_footprint_joint_base_footprint_추가하기.md](04_14_base_footprint_joint_base_footprint_추가하기.md) |
| 04-15 | collision_태그_추가하기 | [04_15_collision_태그_추가하기.md](04_15_collision_태그_추가하기.md) |
| 04-16 | inertial_태그_추가하기 | [04_16_inertial_태그_추가하기.md](04_16_inertial_태그_추가하기.md) |
