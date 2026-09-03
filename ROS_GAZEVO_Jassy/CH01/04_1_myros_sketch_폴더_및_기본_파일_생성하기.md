# 04_1 myros_sketch 폴더 및 기본 파일 생성하기

다음은 ROS2 프로그래밍 파일을 저장할 myros_sketch 폴더를 생성한 후, 로봇 관련 파일을 생성합니다.

1. 명령을 수행합니다.

```bash
mkdir ~/myros_sketch
cd ~/myros_sketch
touch robot.urdf.xacro
touch robot_core_diffdrive.xacro
touch inertial_macros.xacro
touch ros2_control_diffdrive_gz.xacro
touch diffdrive_controllers.yaml
touch gz_bridge.yaml
touch lidar.xacro
touch camera.xacro
```

> ※ 여기서 수행하는 명령은 VS Code 터미널 또는 PowerShell에서 ros_jazzy1에 접속해서 합니다.

2. VS Code에서 myros_sketch 폴더와 생성된 파일을 확인합니다.

![차동 구동 로봇](../img/image61.bmp)

3. 3개의 명령창을 준비합니다.

![폴더 구조 확인](../img/image48.bmp)

![폴더 구조 확인](../img/image62.bmp)

> ※ 이후에 이 3개의 명령창에 다음 명령들을 차례대로 수행하며 실습을 진행합니다.

```
ros2 run robot_state_publisher robot_state_publisher --ros-args -p robot_description:="$(xacro /root/myros_sketch/robot.urdf.xacro)"
rviz2 -d /root/myros_sketch/view_bot.rviz
ros2 run joint_state_publisher_gui joint_state_publisher_gui
```
