## 04_14 base_footprint_joint, base_footprint 추가하기

base_footprint는 로봇의 지면 기준 좌표계를 나타내는 프레임으로, SLAM이나 내비게이션에서 로봇의 위치 계산과 경로 계획에 사용됩니다.

1. 내용을 계속해서 추가합니다.

**robot.urdf.xacro**

```xml
    ...

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

4. 표시되는 것을 확인합니다.

![base_footprint 시각화](../img/image107.bmp)

5. RobotModel에서 Visual Enabled 항목을 언체크하고, Collision Enabled 항목을 체크해 봅니다.

![Collision 모드](../img/image108.bmp)

6. 표시되는 것을 확인합니다.

![Collision 시각화](../img/image109.bmp)

