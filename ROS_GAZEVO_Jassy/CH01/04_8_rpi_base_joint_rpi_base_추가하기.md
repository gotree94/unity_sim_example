# 04_8 rpi_base_joint, rpi_base 추가하기

rpi_base에는 라즈베리파이 5를 장착합니다.

## 1. 내용을 계속해서 추가합니다.

**robot_core_diffdrive.xacro**

```xml
    ...

    <!-- RPI BASE -->

    <joint name="rpi_base_joint" type="fixed">
        <parent link="chassis"/>
        <child link="rpi_base"/>
        <origin xyz="0 0 0.0566" rpy="0 0 0"/>
    </joint>
    <link name="rpi_base">
        <visual>
            <geometry>                
                <box size="0.12 0.12 0.0016"/>
            </geometry>
            <material name="white"/>
        </visual>
    </link>

</robot>
```

## 2. URDF 파일을 저장한 후, 명령을 재구동합니다.

```
ros2 run robot_state_publisher robot_state_publisher --ros-args -p robot_description:="$(xacro /root/myros_sketch/robot.urdf.xacro)"
```

## 3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

![rpi_base 시각화](../img/image83.bmp)

## 4. 표시되는 것을 확인합니다.

![rpi_base 시각화](../img/image103.bmp)
