## 04_13 camera_optical_joint, camera_link_optical 추가하기

카메라 렌즈 부분입니다.

1. 내용을 계속해서 추가합니다.

**robot.urdf.xacro**

```xml
    ...        

    <!-- CAMERA OPTICAL -->
    
    <joint name="camera_optical_joint" type="fixed">
        <parent link="camera_link"/>
        <child link="camera_link_optical"/>
        <origin xyz="0 0 0" rpy="${-pi/2} 0 ${-pi/2}"/>
    </joint>

    <link name="camera_link_optical">
    </link>

</robot>
```

camera_link_optical 링크의 경우, 렌즈의 면이 +z 축을 보고 렌즈의 하단이 +y축을 보고, 렌즈의 오른쪽이 +x축을 보도록 해야 합니다.

| 축 | 의미 |
|---|---|
| +z | 카메라가 보는 방향(렌즈가 향하는 쪽, optical axis) |
| +x | 이미지 기준 오른쪽 |
| +y | 이미지 기준 아래쪽 |

![카메라 좌표계](../img/image105.bmp)

> 이 규약이 따로 존재하는 이유는, 일반 로봇 프레임은 +x 전방, +y 좌측, +z 상방을 쓰는데, 이건 이미지 처리 파이프라인(OpenCV 등)이 기대하는 좌표계와 축이 완전히 다르기 때문입니다.

2. URDF 파일을 저장한 후, 명령을 재구동합니다.

3. RViz2 좌측 하단에 있는 reset 버튼을 누릅니다.

4. 표시되는 것을 확인합니다.

![camera_link_optical 시각화](../img/image106.bmp)


