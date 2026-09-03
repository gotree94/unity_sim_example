## 04_7 imu_joint, imu_link 추가하기

1. 내용을 계속해서 추가합니다.

**robot_core_diffdrive.xacro**

```xml
    ...

    <!-- IMU -->

    <joint name="imu_joint" type="fixed">
        <parent link="chassis"/>
        <child link="imu_link"/>
        <origin xyz="0 0 0.008" rpy="0 0 0"/>
    </joint>
    <link name="imu_link">
        <visual>
            <geometry>
                <box size="0.0155 0.02 0.0126"/>
            </geometry>
            <material name="green"/>
        </visual>
    </link>

</robot>
```

![IMU 규격](../img/image97.bmp)

이 책에서 사용하는 IMU는 MPU6050으로 가로 세로 크기는 다음과 같고, 높이는 1.26cm입니다. MPU6050의 중심은 메인 보드의 중심과 같습니다.

2. URDF 파일을 저장한 후, 명령을 재구동합니다.

3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

4. 표시되는 것을 확인합니다.

![IMU 시각화](../img/image98.bmp)

