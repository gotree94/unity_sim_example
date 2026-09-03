# 04_12 camera_joint, camera_link 추가하기

카메라 몸체입니다.

## 1. 내용을 계속해서 추가합니다.

**robot.urdf.xacro**

```xml
    ...

    <!-- CAMERA FRAME -->

    <joint name="camera_joint" type="fixed">
        <parent link="lidar_base"/>
        <child link="camera_link"/>
        <origin xyz="0.055 0 -0.02" rpy="0 0 0"/>
    </joint>

    <link name="camera_link">
        <visual>        
            <geometry>
                <box size="0.01 0.034 0.04"/>
            </geometry>
            <material name="orange"/>
        </visual>
    </link>

</robot>
```

## 2. URDF 파일을 저장한 후, 명령을 재구동합니다.

```
ros2 run robot_state_publisher robot_state_publisher --ros-args -p robot_description:="$(xacro /root/myros_sketch/robot.urdf.xacro)"
```

## 3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

![camera_link 시각화](../img/image83.bmp)

## 4. 표시되는 것을 확인합니다.

![camera_link 시각화](../img/image108.bmp)

