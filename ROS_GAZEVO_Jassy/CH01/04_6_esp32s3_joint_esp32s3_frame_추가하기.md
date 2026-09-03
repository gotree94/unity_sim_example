# 04_6 esp32s3_joint, esp32s3_frame 추가하기

다음은 esp32s3_joint, esp32s3_frame을 추가합니다.

## 1. 내용을 계속해서 추가합니다.

**robot_core_diffdrive.xacro**

```xml
    ...

    <!-- ESP32S3DEVKITC1 -->

    <joint name="esp32s3_joint" type="fixed">
        <parent link="chassis"/>
        <child link="esp32s3_frame"/>
        <origin xyz="-0.045 0 0.008" rpy="0 0 0"/>
    </joint>
    <link name="esp32s3_frame">
        <visual>
            <geometry>
                <box size="0.056 0.028 0.0126"/>
            </geometry>
            <material name="black"/>
        </visual>
    </link>

</robot>
```

![ESP32S3 규격](../img/image99.bmp)

ESP32S3DevKitC의 중심은 메인 보드의 중심으로부터 -X 방향으로 4.5cm 떨어져 있습니다.

## 2. URDF 파일을 저장한 후, 명령을 재구동합니다.

```
ros2 run robot_state_publisher robot_state_publisher --ros-args -p robot_description:="$(xacro /root/myros_sketch/robot.urdf.xacro)"
```

## 3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

![ESP32S3 시각화](../img/image83.bmp)

## 4. 표시되는 것을 확인합니다.

![ESP32S3 시각화](../img/image100.bmp)

