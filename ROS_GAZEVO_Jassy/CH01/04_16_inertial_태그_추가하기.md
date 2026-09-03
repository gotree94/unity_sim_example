## 04_16 inertial 태그 추가하기

inertial 태그는 링크의 질량과 관성 모멘트, 그리고 무게중심 위치(origin)를 정의하는 요소로, 가제보 물리 엔진이 로봇의 움직임과 힘의 반응을 계산하는 데 사용됩니다.

1. 파일을 작성합니다.

**inertial_macros.xacro**

```xml
<?xml version="1.0"?>
<robot xmlns:xacro="http://www.ros.org/wiki/xacro" >

    <!-- Specify some standard inertial calculations -->
    <!-- https://en.wikipedia.org/wiki/List_of_moments_of_inertia -->

    <xacro:macro name="inertial_sphere" params="mass radius *origin">
        <inertial>
            <xacro:insert_block name="origin"/>
            <mass value="${mass}" />
            <inertia ixx="${(2/5) * mass * (radius*radius)}" ixy="0.0" ixz="0.0"
                    iyy="${(2/5) * mass * (radius*radius)}" iyz="0.0"
                    izz="${(2/5) * mass * (radius*radius)}" />
        </inertial>
    </xacro:macro>  


    <xacro:macro name="inertial_box" params="mass x y z *origin">
        <inertial>
            <xacro:insert_block name="origin"/>
            <mass value="${mass}" />
            <inertia ixx="${(1/12) * mass * (y*y+z*z)}" ixy="0.0" ixz="0.0"
                    iyy="${(1/12) * mass * (x*x+z*z)}" iyz="0.0"
                    izz="${(1/12) * mass * (x*x+y*y)}" />
        </inertial>
    </xacro:macro>


    <xacro:macro name="inertial_cylinder" params="mass length radius *origin">
        <inertial>
            <xacro:insert_block name="origin"/>
            <mass value="${mass}" />
            <inertia ixx="${(1/12) * mass * (3*radius*radius + length*length)}" ixy="0.0" ixz="0.0"
                    iyy="${(1/12) * mass * (3*radius*radius + length*length)}" iyz="0.0"
                    izz="${(1/2) * mass * (radius*radius)}" />
        </inertial>
    </xacro:macro>

</robot>
```

> 본 관성(inertial) 계산 매크로는 ROS2 커뮤니티에 널리 알려진 표준 패턴을 기반으로 하며, Articulated Robotics 튜토리얼의 구성을 참고했습니다.

2. 예제를 수정합니다. (전체 robot_core_diffdrive.xacro 파일에 inertial 태그가 추가됩니다.)

3. URDF 파일을 저장한 후, 명령을 재구동합니다.

4. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

5. RobotModel에서 Visual Enabled 항목과 Collision Enabled 항목을 언체크하고, Mass Properties의 Mass 항목은 체크, Inertia 항목은 언체크하고 결과를 확인합니다.

![Mass 시각화](../img/image112.bmp)

6. Visual Enabled 항목과 Collision Enabled 항목을 언체크하고, Mass Properties의 Mass 항목은 언체크, Inertia 항목은 체크하고 결과를 확인합니다.

![Inertia 시각화](../img/image113.bmp)

7. Visual Enabled 항목은 체크, Collision Enabled 항목은 언체크, Mass Properties의 Mass 항목과 Inertia 항목은 언체크하여 최초 상태로 돌려 놓습니다.

![최초 상태 복원](../img/image114.bmp)

8. 표시되는 것을 확인합니다.

![최종 URDF 시각화](../img/image115.bmp)
