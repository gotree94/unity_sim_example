## 04_11 laser_joint, laser_frame 추가하기

라이다와 관련된 laser_joint, laser_frame을 추가합니다.

1. 내용을 계속해서 추가합니다.

**robot_core_diffdrive.xacro**

```xml
    ...

    <!-- LIDAR -->

    <joint name="laser_joint" type="fixed">
        <parent link="lidar_base"/>
        <child link="laser_frame"/>
        <origin xyz="0 0 0.047" rpy="0 0 0"/>
    </joint>

    <link name="laser_frame">
        <visual>
            <geometry>
                <cylinder radius="0.0296" length="0.016"/>
            </geometry>
            <material name="red"/>
        </visual>
    </link>

</robot>
```

라이다 높이는 2.6cm, 지지대는 4cm입니다.

![라이다 규격](../img/image102.bmp)

2. URDF 파일을 저장한 후, 명령을 재구동합니다.

3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

4. 표시되는 것을 확인합니다.

![laser_frame 시각화](../img/image103.bmp)

