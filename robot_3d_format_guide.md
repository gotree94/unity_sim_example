# 로봇 3D 기술 포맷 가이드: URDF / SDF / USD / MJCF

> 로봇을 표현하는 3D 파일 포맷의 발전사, 최신 기술, 그리고 생성 위저드(Python) 코드 정리
> 작성일: 2026-08-13

---

## 목차
1. [발전 단계별 히스토리](#1-발전-단계별-히스토리)
2. [포맷별 상세 비교](#2-포맷별-상세-비교)
3. [최신 기술 트렌드 정리](#3-최신-기술-트렌드-정리)
4. [생성 위저드 (Python 코드)](#4-생성-위저드-python-코드)
5. [참고 자료 링크](#5-참고-자료-링크)

---

## 1. 발전 단계별 히스토리

### 1단계: 3D 포맷의 초기 (1990년대 ~ 2000년대 초)
| 년도 | 항목 | 내용 |
|---|---|---|
| 1994 | **VRML 1.0** | 웹 3D 장면 표준화 시도. 텍스트 기반 `.wrl` |
| 1995 | **VRML 2.0 / VRML97** | 애니메이션·센서 기능 추가 |
| 1996 | **Wavefront OBJ** | 단순 폴리곤 메시. 도구 의존도 낮음, 오늘날까지 메시 교환 표준으로 사용 |
| 1999 | **STL (Standard Tessellation Language)** | 3D 프린팅·CAD용 삼각형 메시. 정점 정보 없음(순수 삼각형) |
| 2000 | **Collada (DAE)** | Khronos 주도 XML 기반 디지털 에셋 교환 포맷. 관절·애니메이션 포함 |

> **의의**: 이 시절 포맷은 "렌더링"에 집중. 로봇의 기구학·물리를 표현할 방법이 없어 각 시뮬레이터가 고유 포맷을 만들기 시작.

### 2단계: 로봇 전용 포맷의 등장 (2006 ~ 2011)
| 년도 | 항목 | 내용 |
|---|---|---|
| 2006 | **URDF v1.0** | ROS(Gazebo 프로젝트에서 파생된 Player/Stage 계열)에서 정의. XML. 단일 로봇의 링크·조인트·관성·시각 메시 표현 |
| 2007 | **URDF 확장 (Xacro)** | 반복·파라미터·수식 지원을 위한 매크로 언어. URDF의 사실상 표준 상위 포맷 |
| 2008 | **Gazebo 1.x** | URDF를 사용하던 초기 Gazebo. 물리 표현 한계가 드러나기 시작 |
| 2009 | **SDF 1.0** | Gazebo 개발진이 URDF의 한계(월드 표현 불가, 물리 파라미터 빈약)를 극복하기 위해 설계. 월드·조명·센서·물리를 포함한 전체 장면 표현 |
| 2010 | **URDF→SDF 변환기** | `urdf_to_sdf` 등장. URDF 모델을 SDF 모델로 자동 변환 |

> **의의**: 로봇 커뮤니티 최초의 표준화 시도. URDF는 "설계/표시", SDF는 "시뮬레이션"으로 역할 분담.

### 3단계: 물리 엔진 전용 포맷 (2011 ~ 2015)
| 년도 | 항목 | 내용 |
|---|---|---|
| 2011 | **MuJoCo 1.0 / MJCF** | DeepMind 출신의 물리엔진. 콤팩트한 MJCF XML로 관절·접촉·자유도를 선언적으로 표현. 연산 속도가 결정적 장점 |
| 2013 | **Bullet의 .urdf 확장** | PyBullet이 URDF를 기반으로 하고, 접촉·관절 스프링 등의 확장 파라미터 추가 |
| 2015 | **CoppeliaSim (V-REP) 포맷** | 시뮬레이터 고유 씬 포맷(.ttt) 및 `.sdf`/`.urdf` 임포트 지원 |

### 4단계: 필름 산업 포맷의 유입 (2016 ~ 2020)
| 년도 | 항목 | 내용 |
|---|---|---|
| 2016 | **USD 0.x (OpenUSD 초기)** | 픽사가 씬 그래프·조립·버전 관리를 위한 **Universal Scene Description** 개발. 자체 렌더러 Hydra, 재질 MaterialX |
| 2019 | **USD 20.x 오픈소스화** | 픽사가 Apache 2.0 라이선스로 공개 |
| 2020 | **USD 21.x / .usdz** | Apple AR Quick Look에 `.usdz` 채택, 엔터테인먼트+AR 확산 |
| 2020 | **NVIDIA Omniverse 발표** | USD를 핵심 데이터 포맷으로 채택한 협업·시뮬레이션 플랫폼 |
| 2020 | **Isaac Sim** | USD 기반 로봇 시뮬레이터 출시. URDF·SDF·MJCF를 USD로 변환하는 어댑터 포함 |

### 5단계: 표준 수렴과 통합 (2021 ~ 현재)
| 년도 | 항목 | 내용 |
|---|---|---|
| 2021 | **MJX** | MuJoCo를 JAX 기반 GPU 가속으로 재작성. 대규모 병렬 로봇 학습 가능 |
| 2022 | **ROS 2 + Gazebo Fortress/Harmonic** | SDF가 Ignition (지금의 Gazebo) 에디션으로 계속 발전. `gz sim` 표준 |
| 2022 | **OpenUSD Alliance 출범** | Pixar, NVIDIA, Apple, Autodesk 등 다수 업체 합류. USD의 사실상 업계 표준화 |
| 2023 | **USD 23.08/23.11** | 데이터 모델 안정화, Hydra 2.0 가속, 실시간 워크플로 강화 |
| 2024 | **Isaac Lab / Isaac Sim 4.0** | `simulation.app` 대신 `simulation.app` 통합 아키텍처. USD 기반 장면이 디폴트 |
| 2025 | **OpenUSD 24.x/25.x** | glTF 2.0, MaterialX 1.38 지원 강화. USD-Asset-Resolver 통합 |
| 2026 | **최신 흐름** | AI 로보틱스 에이전트 학습 데이터(Parkour, robotic-vla 등)에서 USD+MJCF+Isaac의 결합이 사실상 표준. 한편 경량 웹 표시는 glTF/GLB가 대세 |

---

## 2. 포맷별 상세 비교

| 항목 | URDF | SDF | MJCF | USD |
|---|---|---|---|---|
| **주 사용처** | RViz 표시, ROS | Gazebo 시뮬레이션 | MuJoCo / Isaac Lab | NVIDIA Isaac/Omniverse, VFX |
| **포맷** | XML | XML | XML | USDA/USDC/USDZ (텍스트/바이너리/패키지) |
| **장면 규모** | 단일 로봇 | 전체 월드(다중 모델) | 단일 모델+간단 월드 | 무한 조립(composition) |
| **물리 표현** | 관성·조인트 한계만 | 관성·마찰·접촉·조인트 | 고급 접촉·제약·관절 최적화 | 물리 속성은 별도 계약/오버라이드 |
| **재질** | 부분(gazebo tag) | Material | Texture | MaterialX (PBR 표준) |
| **센서/플러그인** | X | O (강력) | 제한적 | 확장 아키텍처 |
| **버전 관리** | 비공식 | 1.x ~ 1.11 | MJCF 3.x | USD 버전 + 씬 합성(오버라이드) |
| **상호 변환** | →SDF 자동, →USD 어댑터 | ↔USD 어댑터 | →USD 어댑터 (Isaac) | →glTF/OBJ/DAE 내보내기 |

### 기타 인접 포맷
- **glTF 2.0 / GLB**: Khronos 표준. PBR 재질, 애니메이션. 웹·모바일 표시용으로 확산. USD ↔ glTF 브릿지 존재.
- **Xacro**: URDF 상위 매크로 언어 (`<xacro:macro>`).
- **STEP/IGES**: CAD 교환. 시뮬레이션용 아님.
- **X3D**: VRML의 후계 표준(ISO).
- **FBX**: 게임·애니메이션 산업 표준. 로봇 시뮬레이션엔 거의 미사용.

---

## 3. 최신 기술 트렌드 정리

1. **USD가 허브가 되는 변환 생태계**
   - URDF·SDF·MJCF·glTF를 USD로 통합하는 어댑터가 표준화됨.
   - 예: `urdf_to_usd` (Isaac Utils), `sdf_to_usd`, `mujoco→USD` 변환기.

2. **물리 기반 재질 (MaterialX) + 렌더링 분리 (Hydra)**
   - USD는 장면을 "데이터"로, 렌더링을 "Hydra"로 분리. 로봇 시뮬레이션에서도 photorealistic 재질 중요.

3. **GPU 병렬 물리 시뮬레이션**
   - MuJoCo MJX(전체 관절 GPU), NVIDIA PhysX 5, Isaac Sim의 비동기 물리.
   - 대규모 병렬 로봇 학습(RL)이 주목적.

4. **Foundation Model + Embodied AI 데이터 파이프라인**
   - 로봇 정책 학습용 대규모 에셋: OpenUSD 기반 장면 + MJCF/PhysX를 결합한 재현 가능한 데이터셋.

5. **웹/AR 표시는 glTF**
   - 브라우저·AR(Apple/Google)은 USDZ·glTF로 표시. ROS 웹비주얼라이저(Rosbridge + three.js)는 glTF 로드.

6. **ROS 2 표준 포맷 유지**
   - `robot_description`은 여전히 URDF(Xacro)가 디폴트. 다만 `ros2_control` 등은 URDF를 내부 파라미터로 사용.
   - Gazebo (Ignition)의 표준은 여전히 SDF 1.11+.

---

## 4. 생성 위저드 (Python 코드)

아래 코드는 **명령줄 대화형 위저드**로 사용자가 입력한 값을 기반으로 URDF, SDF, USD 파일을 생성합니다.
외부 라이브러리 **없이** 표준 라이브러리만 사용하도록 작성했습니다 (USD는 텍스트 USDA를 직접 생성).

### 4-1. 단일 스크립트: `robot_wizard.py`

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
로봇 설명 파일 생성 위저드
: URDF / SDF / USD(USDA) 를 대화형 입력으로 생성합니다.

사용법:
    python robot_wizard.py
생성 예:
    python robot_wizard.py --format urdf --name my_robot
옵션:
    --format {urdf,sdf,usd}  지정된 포맷만 생성 (기본: 모두)
    --name <name>            로봇 이름 (기본: my_robot)
    --out <dir>              출력 디렉터리 (기본: ./output)
"""

import argparse
import os
import sys
from pathlib import Path


def ask(prompt, default=None):
    """대화형 입력. 엔터만 누르면 default 사용."""
    if default is not None:
        user = input(f"{prompt} [기본값: {default}] > ").strip()
        return user if user else default
    while True:
        user = input(f"{prompt} > ").strip()
        if user:
            return user
        print("값을 입력해 주세요.")


def ask_float(prompt, default=0.0):
    """실수 입력 파서."""
    while True:
        raw = ask(prompt, default)
        try:
            return float(raw)
        except ValueError:
            print("숫자를 입력해 주세요.")


def gather_input():
    """위저드 입력 수집 (dict 반환)."""
    print("\n" + "=" * 60)
    print(" 로봇 설명 파일 생성 위저드")
    print("=" * 60)

    data = {
        "name": ask("로봇 이름", "my_robot"),
        "link_name": ask("메인 링크 이름", "base_link"),
        "joint_name": ask("관절 이름", "joint1"),
        "parent_link": ask("부모 링크", "base_link"),
        "child_link": ask("자식 링크", "arm_link"),
        "mesh_file": ask("메시 파일(OBJ/DAE/STL)", "mesh.obj"),
        "x": ask_float("링크 크기 X [m]", 0.2),
        "y": ask_float("링크 크기 Y [m]", 0.2),
        "z": ask_float("링크 크기 Z [m]", 0.2),
        "mass": ask_float("링크 질량 [kg]", 1.0),
        "joint_type": ask("관절 타입(revolute/prismatic/fixed)", "revolute"),
        "axis": ask("관절 축(x/y/z)", "z"),
    }

    # 메시 파일 유무에 따라 (메시) or (박스) 선택
    data["use_mesh"] = os.path.exists(data["mesh_file"])
    if not data["use_mesh"]:
        print(f"  참고: '{data['mesh_file']}' 이 없어 박스(Box) 형태로 생성합니다.")

    return data


# ---------------------------------------------------------------------------
# URDF 생성
# ---------------------------------------------------------------------------
def build_urdf(d):
    return f"""<?xml version="1.0"?>
<robot name="{d['name']}">

  <link name="{d['link_name']}">
    <inertial>
      <mass value="{d['mass']}"/>
      <origin xyz="0 0 0" rpy="0 0 0"/>
      <inertia ixx="0.01" ixy="0.0" ixz="0.0"
               iyy="0.01" iyz="0.0"
               izz="0.01"/>
    </inertial>
    <visual>
      <geometry>
        {f"<mesh filename=\"{d['mesh_file']}\"/>" if d["use_mesh"] else f"<box size=\"{d['x']} {d['y']} {d['z']}\"/>"}
      </geometry>
      <material name="white">
        <color rgba="1 1 1 1"/>
      </material>
    </visual>
    <collision>
      <geometry>
        <box size="{d['x']} {d['y']} {d['z']}"/>
      </geometry>
    </collision>
  </link>

  <link name="{d['child_link']}">
    <inertial>
      <mass value="{d['mass'] * 0.5:.3f}"/>
      <origin xyz="0 0 0" rpy="0 0 0"/>
      <inertia ixx="0.005" ixy="0.0" ixz="0.0"
               iyy="0.005" iyz="0.0"
               izz="0.005"/>
    </inertial>
    <visual>
      <geometry>
        <box size="{d['x'] / 2} {d['y'] / 2} {d['z'] / 2}"/>
      </geometry>
    </visual>
  </link>

  <joint name="{d['joint_name']}" type="{d['joint_type']}">
    <parent link="{d['parent_link']}"/>
    <child link="{d['child_link']}"/>
    <origin xyz="0 0 {d['z'] / 2}"/>
    <axis xyz="0 0 1" />
    <limit lower="-1.57" upper="1.57" effort="10" velocity="1.0"/>
    <dynamics damping="0.1" friction="0.1"/>
  </joint>

</robot>
"""


# ---------------------------------------------------------------------------
# SDF 생성
# ---------------------------------------------------------------------------
def build_sdf(d):
    return f"""<?xml version="1.0"?>
<sdf version="1.11">
  <world name="default">
    <include>
      <uri>model://ground_plane</uri>
      <name>ground_plane</name>
    </include>
    <include>
      <uri>model://sun</uri>
      <name>sun</name>
    </include>

    <model name="{d['name']}">
      <pose>0 0 0 0 0 0</pose>
      <link name="{d['link_name']}">
        <pose>0 0 0 0 0 0</pose>
        <inertial>
          <mass>{d['mass']}</mass>
          <inertia>
            <ixx>0.01</ixx><iyy>0.01</iyy><izz>0.01</izz>
            <ixy>0</ixy><ixz>0</ixz><iyz>0</iyz>
          </inertia>
        </inertial>
        <collision name="collision_0">
          <geometry>
            <box><size>{d['x']} {d['y']} {d['z']}</size></box>
          </geometry>
        </collision>
        <visual name="visual_0">
          <geometry>
            {f"<mesh><uri>{d['mesh_file']}</uri></mesh>" if d["use_mesh"] else f"<box><size>{d['x']} {d['y']} {d['z']}</size></box>"}
          </geometry>
          <material>
            <ambient>0.9 0.9 0.9 1</ambient>
            <diffuse>0.9 0.9 0.9 1</diffuse>
          </material>
        </visual>
      </link>

      <link name="{d['child_link']}">
        <pose>0 0 {d['z'] / 2} 0 0 0</pose>
        <inertial>
          <mass>{d['mass'] * 0.5:.3f}</mass>
        </inertial>
      </link>

      <joint name="{d['joint_name']}" type="{d['joint_type']}">
        <parent>{d['parent_link']}</parent>
        <child>{d['child_link']}</child>
        <pose>0 0 0 0 0 0</pose>
        <axis>
          <xyz>0 0 1</xyz>
          <limit>
            <lower>-1.57</lower><upper>1.57</upper>
            <effort>10</effort><velocity>1.0</velocity>
          </limit>
        </axis>
      </joint>
    </model>
  </world>
</sdf>
"""


# ---------------------------------------------------------------------------
# USD (USDA) 생성
# ---------------------------------------------------------------------------
def build_usd(d):
    axis = {"x": "1,0,0", "y": "0,1,0", "z": "0,0,1"}.get(d["axis"], "0,0,1")
    return f"""{d['name']} (USDA 예시)

def Xform "{d['name']}" (
    kind = "component"
)
{{
    def Xform "{d['link_name']}"
    {{
        def Mesh "visual_mesh"
        {{
            uniform int[] faceVertexCounts = [4]
            uniform int[] faceVertexIndices = [0, 1, 2, 3]
            point3f[] points = [
                (-{d['x']/2}, -{d['y']/2}, -{d['z']/2}),
                ( {d['x']/2}, -{d['y']/2}, -{d['z']/2}),
                ( {d['x']/2},  {d['y']/2}, -{d['z']/2}),
                (-{d['x']/2},  {d['y']/2}, -{d['z']/2})
            ]
            uniform token[] xformOpOrder = []
        }}
        float3 xformOp:translate = (0, 0, 0)
        uniform token[] xformOpOrder = ["xformOp:translate"]
    }}

    # 로봇 관절 정보는 USD "계약(convention)"으로 표현됩니다.
    # (Isaac/ROS2에서는 usd_physics, usd_urdf 같은 스키마가 사용됨)
    def Xform "{d['joint_name']}"
    {{
        float3 xformOp:translate = (0, 0, {d['z']/2})
        uniform token[] xformOpOrder = ["xformOp:translate"]
        rel  usdPhysics:joint1 = <{d['child_link']}>
        rel  usdPhysics:joint2 = <{d['parent_link']}>
        uniform token jointType = "{d['joint_type']}"
        uniform token axis = "{d['axis']}"
    }}
}}
"""
# ─ 주의: 위는 USDA 문법을 학습 목적으로 단순화한 예시입니다.
#   실제 Isaac에서 사용하려면 다음 스키마를 레퍼런스하세요:
#     `usdPhysics:articulationRoot:articulations`,
#     `usdPhysics:joint:limit:lower/upper`, `PhysicsRevoluteJoint` 등.


# ---------------------------------------------------------------------------
# 실행부
# ---------------------------------------------------------------------------
def main():
    parser = argparse.ArgumentParser(description="로봇 설명 파일 생성 위저드")
    parser.add_argument("--format", choices=["urdf", "sdf", "usd"],
                        help="생성할 포맷 (기본: 모두)")
    parser.add_argument("--name", help="로봇 이름")
    parser.add_argument("--out", default="output", help="출력 디렉터리")
    args = parser.parse_args()

    d = gather_input()
    if args.name:
        d["name"] = args.name

    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    builders = {
        "urdf": (build_urdf, ".urdf"),
        "sdf": (build_sdf, ".sdf"),
        "usd": (build_usd, ".usda"),
    }

    selected = [args.format] if args.format else list(builders)
    for fmt in selected:
        fn, ext = builders[fmt]
        path = out / f"{d['name']}{ext}"
        path.write_text(fn(d), encoding="utf-8")
        print(f"[OK] {fmt} -> {path}")

    print("\n생성 완료. RViz/Gazebo/Isaac에서 로드해 보세요.")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n중단됨.")
        sys.exit(0)
```

### 4-2. 실행 예시

```bash
# 1) 모두 생성
python robot_wizard.py

# 2) URDF만, 이름 지정, 출력 폴더 지정
python robot_wizard.py --format urdf --name r2d2 --out c:\robot_output
```

대화형 실행 화면:
```
============================================================
 로봇 설명 파일 생성 위저드
============================================================
로봇 이름 [기본값: my_robot] > r2d2
메인 링크 이름 [기본값: base_link] > base_link
...
[OK] urdf -> output\r2d2.urdf
[OK] sdf  -> output\r2d2.sdf
[OK] usd  -> output\r2d2.usda
```

### 4-3. 각 포맷의 실제 사용 워크플로

```bash
# URDF: RViz에서 확인
ros2 launch urdf_tutorial display.launch.py model:=r2d2.urdf

# SDF: Gazebo (Ignition)에서 확인
ign gazebo -s r2d2.sdf          # (구버전 명령)
gz sim r2d2.sdf                 # Gazebo Harmonic

# USD: Isaac Sim에서 확인
python scripts/run_headless_python.py --scene r2d2.usda

# 공식 변환 도구 (최신)
#  URDF -> USD : `isaac_ros_urdf2usd` (NVIDIA 컨테이너)
#  SDF  -> USD : `gz usd` (Ignition USD 프러그인)
#  MJCF -> USD : `mujoco_menagerie` + 컨버터
```

> **주의**: 위 위저드는 교육용 "박스 로봇"만 생성합니다.
> 실제 로봇을 만들려면 다음 오픈소스 에셋을 참고하세요.
> - URDF: ROS `urdf_tutorial`, `kinova_description`
> - SDF: `gz-sim` 예제, `sdformat` 레포
> - USD: NVIDIA `isaac_robot_examples`, Mujoco Menagerie (USD 변환본)

---

## 5. 참고 자료 링크

| 주제 | 링크 |
|---|---|
| URDF 공식 문서 (ROS 2) | https://docs.ros.org/en/rolling/ Tutorials/Tutorials/URDF/ |
| URDF spec (ROS wiki) | http://wiki.ros.org/urdf/XML |
| Xacro 문서 | http://wiki.ros.org/xacro |
| SDFormat 공식 문서 | http://sdformat.org |
| Gazebo Sim 문서 | https://gazebosim.org/docs |
| OpenUSD 공식 사이트 | https://openusd.org |
| USD 다운로드/설치 | https://github.com/PixarAnimationStudios/OpenUSD |
| NVIDIA Isaac Sim | https://docs.isaacsim.omniverse.nvidia.com |
| NVIDIA URDF→USD 튜토리얼 | https://docs.isaacsim.omniverse.nvidia.com/latest/isaac_ros_tutorials/.../tutorial_isaac_ros_urdf2usd.html |
| MuJoCo 문서 | https://mujoco.org |
| Mujoco Menagerie (로봇 모델 모음) | https://github.com/google-deepmind/mujoco_menagerie |
| glTF 2.0 규격 | https://github.com/KhronosGroup/glTF |
| ROS + USD 브릿지 | https://github.com/irosy/ROS2_USD |
| USD → glTF 컨버터 | https://github.com/google/usd2gltf |

---

*본 문서는 교육용으로 작성되었습니다. 라이선스: MIT*
