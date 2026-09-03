## 04_10 lidar_base_joint, lidar_base_frame 추가하기

라이다가 장착된 lidar_base_joint, lidar_base를 추가합니다.

1. 내용을 계속해서 추가합니다.

**robot_core_diffdrive.xacro**

```xml
    ...

    <!-- LIDAR BASE -->

    <joint name="lidar_base_joint" type="fixed">
        <parent link="rpi_base"/>
        <child link="lidar_base"/>
        <origin xyz="0 0 0.0566" rpy="0 0 0"/>
    </joint>
    <link name="lidar_base">
        <visual>
            <geometry>                
                <box size="0.12 0.12 0.0016"/>
            </geometry>
            <material name="white"/>
        </visual>
    </link>

</robot>
```

2. URDF 파일을 저장한 후, 명령을 재구동합니다.

3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

4. 표시되는 것을 확인합니다.

![lidar_base 시각화](../img/image101.bmp)
