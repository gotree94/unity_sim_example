## 04_15 collision 태그 추가하기

collision 태그는 링크의 충돌 영역을 정의합니다. Gazebo 시뮬레이터에서는 이 정보를 이용하여 링크 간의 충돌 여부를 계산합니다.

1. 예제를 수정합니다.

**robot_core_diffdrive.xacro** (전체 파일)

```xml
<?xml version="1.0"?>
<robot xmlns:xacro="http://www.ros.org/wiki/xacro">

    <xacro:include filename="inertial_macros.xacro"/>

    <material name="white">
        <color rgba="1 1 1 0.5"/>
    </material>

    <material name="orange">
        <color rgba="1 0.3 0.1 1"/>
    </material>

    <material name="blue">
        <color rgba="0.2 0.2 1 0.5"/>
    </material>

    <material name="black">
        <color rgba="0 0 0 1"/>
    </material>

    <material name="green">
        <color rgba="0 1 0 1"/>
    </material>

    <material name="red">
        <color rgba="1 0 0 1"/>
    </material>

    <!-- BASE LINK -->
    <link name="base_link">
    </link>

    <!-- CHASSIS -->
    <joint name="chassis_joint" type="fixed">
        <parent link="base_link"/>
        <child link="chassis"/>
        <origin xyz="0 0 0.0058" rpy="0 0 0"/>
    </joint>

    <link name="chassis">
        <visual>
            <geometry>
                <box size="0.15 0.15 0.0016"/>
            </geometry>
            <material name="white"/>
        </visual>
        <collision>
            <geometry>
                <box size="0.15 0.15 0.0016"/>
            </geometry>
        </collision>
    </link>

    <!-- LEFT WHEEL -->
    <joint name="left_wheel_joint" type="continuous">
        <parent link="base_link"/>
        <child link="left_wheel"/>
        <origin xyz="0 0.0907 0" rpy="-${pi/2} 0 0"/>
        <axis xyz="0 0 1"/>
    </joint>

    <link name="left_wheel">
        <visual>
            <geometry>
                <cylinder length="0.026" radius="0.0325" />
            </geometry>
            <material name="blue"/>
        </visual>
        <collision>
            <geometry>
                <cylinder length="0.026" radius="0.0325" />
            </geometry>
        </collision>
    </link>

    <!-- RIGHT WHEEL -->
    <joint name="right_wheel_joint" type="continuous">
        <parent link="base_link"/>
        <child link="right_wheel"/>
        <origin xyz="0 -0.0907 0" rpy="${pi/2} 0 0"/>
        <axis xyz="0 0 -1"/>
    </joint>

    <link name="right_wheel">
        <visual>
            <geometry>
                <cylinder length="0.026" radius="0.0325" />
            </geometry>
            <material name="blue"/>
        </visual>
        <collision>
            <geometry>
                <cylinder length="0.026" radius="0.0325" />
            </geometry>
        </collision>
    </link>

    <!-- FRONT CASTER -->
    <joint name="front_caster_joint" type="fixed">
        <parent link="chassis"/>
        <child link="front_caster"/>
        <origin xyz="0.0695 0 -0.0333" rpy="0 0 0"/>
    </joint>

    <link name="front_caster">
        <visual>
            <geometry>
                <sphere radius="0.005" />
            </geometry>
            <material name="black"/>
        </visual>
        <collision>
            <geometry>
                <sphere radius="0.005" />
            </geometry>
        </collision>
    </link>

    <!-- REAR CASTER -->
    <joint name="rear_caster_joint" type="fixed">
        <parent link="chassis"/>
        <child link="rear_caster"/>
        <origin xyz="-0.0695 0 -0.0333" rpy="0 0 0"/>
    </joint>

    <link name="rear_caster">
        <visual>
            <geometry>
                <sphere radius="0.005" />
            </geometry>
            <material name="black"/>
        </visual>
        <collision>
            <geometry>
                <sphere radius="0.005" />
            </geometry>
        </collision>
    </link>

    <!-- ESP32S3DEVKITC1 -->
    <joint name="esp32s3_joint" type="fixed">
        <parent link="chassis"/>
        <child link="esp32s3_frame"/>
        <origin xyz="-0.045 0 0.008" rpy="0 0 0"/>
    </joint>
    <link name="esp32s3_frame">
        <visual>
            <geometry>
                <box size="0.056 0.028 0.016"/>
            </geometry>
            <material name="black"/>
        </visual>
        <collision>
            <geometry>
                <box size="0.056 0.028 0.016"/>
            </geometry>
        </collision>
    </link>

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
        <collision>
            <geometry>
                <box size="0.0155 0.02 0.0126"/>
            </geometry>
        </collision>
    </link>

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
        <collision>
            <geometry>                
                <box size="0.12 0.12 0.0016"/>
            </geometry>
        </collision>
    </link>

    <!-- RPI5 -->
    <joint name="rpi_joint" type="fixed">
        <parent link="rpi_base"/>
        <child link="rpi_frame"/>
        <origin xyz="-0.02 0 0.0316" rpy="0 0 0"/>
    </joint>
    <link name="rpi_frame">
        <visual>
            <geometry>
                <box size="0.085 0.049 0.0192"/>
            </geometry>
            <material name="green"/>
        </visual>
        <collision>
            <geometry>
                <box size="0.085 0.049 0.0192"/>
            </geometry>
        </collision>
    </link>

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
        <collision>
            <geometry>                
                <box size="0.12 0.12 0.0016"/>
            </geometry>
        </collision>
    </link>

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
        <collision>
            <geometry>
                <cylinder radius="0.0296" length="0.016"/>
            </geometry>
        </collision>
    </link>    

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
        <collision>        
            <geometry>
                <box size="0.01 0.034 0.04"/>
            </geometry>
        </collision>
    </link>       

    <!-- CAMERA OPTICAL -->
    <joint name="camera_optical_joint" type="fixed">
        <parent link="camera_link"/>
        <child link="camera_link_optical"/>
        <origin xyz="0 0 0" rpy="${-pi/2} 0 ${-pi/2}"/>
    </joint>

    <link name="camera_link_optical">
    </link>

    <!-- BASE_FOOTPRINT LINK -->
    <joint name="base_footprint_joint" type="fixed">
        <parent link="base_footprint"/>
        <child link="base_link"/>
        <origin xyz="0 0 0.0325" rpy="0 0 0"/>
    </joint>

    <link name="base_footprint">
    </link>

</robot>
```

2. URDF 파일을 저장한 후, 명령을 재구동합니다.

3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

4. RobotModel에서 Visual Enabled 항목을 언체크하고, Collision Enabled 항목을 체크해 봅니다.

![Collision 활성화](../img/image110.bmp)

5. 표시되는 것을 확인합니다.

![Collision 시각화](../img/image111.bmp)

