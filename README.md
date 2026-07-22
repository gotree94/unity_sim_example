# Unity 로봇 만들기 튜토리얼 - Isaac Sim 예제 Unity 구현

> **대상**: Unity 초심자  
> **소요 시간**: 약 130 ~ 180분  
> **사용 환경**: Windows 10/11  
> **Unity 버전**: Unity 2022.3 LTS (2022년 9월 ~ )  
> **목표**: Unity에 익숙해진 뒤, NVIDIA Isaac Sim 튜토리얼 2.2 "Add Simple Objects"를 Unity에서 구현하고, 외부 Python 프로그램에서 키보드 입력으로 로봇을 원격 제어하는 기능까지 구현

---

## 목차

1. [Unity 설치하기 (Windows)](#1-unity-설치하기-windows)
2. [Asset Store 예제로 Unity 익히기](#2-asset-store-예제로-unity-익히기)
3. [프로젝트 생성 및 기본 설정](#3-프로젝트-생성-및-기본-설정)
4. [지면(Ground) 만들기](#4-지면ground-만들기)
5. [로봇 몸통(Cube) 만들기](#5-로봇-몸통cube-만들기)
6. [로봇 바퀴(Cylinder) 만들기](#6-로봇-바퀴cylinder-만들기)
7. [물리 효과 적용 (Rigidbody 및 Collider)](#7-물리-효과-적용-rigidbody-및-collider)
8. [충돌 검사 윤곽선 확인](#8-충돌-검사-윤곽선-확인)
9. [접촉 및 마찰 매개변수 (Physic Material)](#9-접촉-및-마찰-매개변수-physic-material)
10. [객체의 색상 변경 (Material)](#10-객체의-색상-변경-material)
11. [키보드 입력으로 로봇 조종하기](#11-키보드-입력으로-로봇-조종하기)
12. [최종 테스트 및 정리](#12-최종-테스트-및-정리)
13. [외부 Python 프로그램과 연결하기](#13-외부-python-프로그램과-연결하기)

---

## 1. Unity 설치하기 (Windows)

> **목적**: Windows PC에 Unity 개발 환경을 완전히 설치  
> **소요 시간**: 약 15~20분  
> **전제 조건**: 인터넷 연결, 디스크 여유 공간 10GB 이상

### 1-1. Unity란?

Unity는 게임 개발부터 로봇 시뮬레이션, VR/AR, 건축 시뮬레이션까지 다양한 분야에서 사용되는 **3D/2D 게임 엔진**입니다. Unreal Engine과 함께 세계에서 가장 많이 사용되는 게임 엔진 중 하나이며, 로봇 공학 분야에서도 시뮬레이션 도구로 널리 사용됩니다.

### 1-2. Unity Hub 설치

Unity Hub는 여러 Unity 버전을 관리하고 프로젝트를 쉽게 생성/관리할 수 있게 해주는 프로그램입니다.

#### 1단계: Unity Hub 다운로드

1. 웹 브라우저를 엽니다.
2. **https://unity.com/download** 에 접속합니다.
3. **"Download Unity Hub"** 버튼을 클릭합니다.
4. **`UnityHubSetup.exe`** 파일이 다운로드됩니다.

#### 2단계: Unity Hub 설치

1. 다운로드된 **`UnityHubSetup.exe`** 파일을 더블클릭하여 실행합니다.
2. 사용자 계정 컨트롤(UAC) 창이 나타나면 **"예"**를 클릭합니다.
3. 라이선스 계약에 동의하고 **"동의하고 설치"**를 클릭합니다.
4. 설치가 완료될 때까지 기다립니다 (약 2~5분).

#### 3단계: Unity Hub 실행 및 로그인

1. Unity Hub가 자동으로 실행됩니다. 실행되지 않으면 시작 메뉴에서 **"Unity Hub"**를 검색하여 실행합니다.
2. **"Sign in"** 버튼을 클릭하여 Unity 계정으로 로그인합니다.
   - Unity 계정이 없다면 **"Create account"**를 클릭하여 무료로 가입합니다.
   - 학교 이메일이 있다면 **Unity Student** 라이선스를 무료로 받을 수 있습니다.

### 1-3. Unity Editor 설치

Unity Hub를 설치했다면 이제 실제 Unity 에디터를 설치해야 합니다.

#### 1단계: Unity 버전 선택

1. Unity Hub 좌측에서 **"Installs"** 탭을 클릭합니다.
2. 상단의 **"Install Editor"** 버튼을 클릭합니다.
3. **"Official Releases"** 탭에서 LTS 버전을 선택합니다.

> ⚠️ **중요**: Unity Hub 최신 버전(2025년 기준)에서는 기본적으로 **Unity 6**만 표시됩니다.  
> **Unity 2022.3 LTS** 버전을 설치하려면 다음 방법을 사용합니다:

**방법 A: Archived 버전 탭 사용 (추천)**

1. "Install Editor" 창에서 상단의 **"Official Releases"** 옆에 있는 탭을 찾아보세요.
2. **"Archived"** 또는 **"Older"** 라는 탭이 있을 수 있습니다.
3. 해당 탭을 클릭하면 이전 LTS 버전들이 표시됩니다.
4. **Unity 2022.3.x LTS** 를 찾아 선택합니다.

**방법 B: Unity Hub 설정에서 이전 버전 표시**

1. Unity Hub 우측 상단의 **설정(톱니바퀴)** 아이콘을 클릭합니다.
2. **"Preferences"** 또는 **"설정"** 을 엽니다.
3. **"Installs"** 또는 **"에디터"** 관련 설정을 찾습니다.
4. **"Show archived versions"** 또는 **"이전 버전 표시"** 옵션을 활성화합니다.
5. 다시 "Install Editor"로 돌아가면 이전 버전이 표시됩니다.

**방법 C: Unity 공식 아카이브에서 직접 다운로드**

1. 웹 브라우저에서 **https://unity.com/releases/editor/archive** 에 접속합니다.
2. **"Unity 2022.3.x LTS"** 를 찾아 **"Unity Hub"** 버튼을 클릭합니다.
3. Unity Hub가 자동으로 열리면서 해당 버전의 설치 화면이 표시됩니다.

> 💡 **팁**: 이 튜토리얼은 **Unity 2022.3 LTS** 버전을 기준으로 작성되었습니다.  
> Unity 6으로 진행해도 대부분 동일하게 작동하지만, 일부 메뉴 위치가 다를 수 있습니다.

#### 2단계: 모듈 선택

Unity 에디터와 함께 설치할 모듈을 선택합니다:

| 모듈 | 선택 여부 | 설명 |
|------|----------|------|
| **Microsoft Visual Studio Community** | ✅ 선택 | C# 코드 편집기 (권장) |
| **Android Build Support** | ❌ 선택 안 함 | Android 빌드용 (이 튜토리얼 불필요) |
| **iOS Build Support** | ❌ 선택 안 함 | iOS 빌드용 (이 튜토리얼 불필요) |
| **Documentation** | ✅ 선택 | 오프라인 문서 |
| **Language Packs** | Korean 선택 | 한국어 언어 팩 |

> 💡 **팁**: Visual Studio가 이미 설치되어 있다면 중복 설치를 피할 수 있습니다.  
> 이 경우 Unity Hub에서 Visual Studio 선택을 해제하고 기존 IDE를 연결해도 됩니다.

#### 3단계: 설치 위치 및 진행

1. **"Continue"** 버튼을 클릭합니다.
2. 설치 위치를 확인합니다 (기본값: `C:\Program Files\Unity\Hub\Editor\`).
3. **"Install"** 버튼을 클릭합니다.
4. 설치가 완료될 때까지 기다립니다 (약 10~20분, 인터넷 속도에 따라 다름).

### 1-4. Visual Studio 설치 (이미 설치되어 있지 않은 경우)

Visual Studio는 Unity에서 C# 스크립트를 작성할 때 사용하는 IDE입니다.

1. Unity Hub에서 모듈로 선택했다면 자동으로 설치됩니다.
2. 수동으로 설치하려면 **https://visualstudio.microsoft.com/ko-kr/downloads/** 에서 **Community** 버전을 다운로드합니다.
3. 설치 시 **"Unity 게임 개발"** 워크로드를 선택합니다.

### 1-5. Unity 에디터 첫 실행

1. Unity Hub의 **"Projects"** 탭으로 돌아갑니다.
2. **"New project"** 버튼을 클릭합니다.
3. Unity 에디터가 처음 열리면 몇 가지 초기 설정이 진행됩니다.
4. **"Dark"** 테마를 선택하는 것을 추천합니다 (눈의 피로 감소).

### 1-6. Windows 방화벽 설정

Python과의 연결을 위해 Unity가 네트워크 통신을 할 수 있어야 합니다.

1. Windows **"시작"** 메뉴에서 **"Windows Defender 방화벽"**을 검색합니다.
2. **"고급 설정"**을 클릭합니다.
3. 좌측의 **"인바운드 규칙"**을 클릭합니다.
4. 우측의 **"새 규칙..."**을 클릭합니다.
5. **"프로그램"** 선택 > **"다음"**
6. 프로그램 경로에 Unity 에디터 경로를 추가합니다:
   ```
   C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Unity.exe
   ```
7. **"연결 허용"** 선택 > **"다음"**
8. 이름을 **"Unity Editor"**로 입력 > **"마침"**

> 💡 **팁**: Python에서 Unity로 연결할 때 "연결 거부" 오류가 나면 이 설정이 되어 있지 않은 경우가 많습니다.

### 1-7. 설치 확인 체크리스트

Unity가 올바르게 설치되었는지 확인합니다:

- [ ] Unity Hub가 실행되는지
- [ ] Unity Hub에서 "Installs" 탭에 Unity 2022.3 LTS가 보이는지
- [ ] Unity 에디터를 열 수 있는지 (Hub에서 "New project" 클릭 시 열리는지)
- [ ] Visual Studio에서 C# 파일을 열 수 있는지
- [ ] Unity 에디터의 Scene/Game/Hierarchy/Inspector/Project 창이 모두 보이는지

### 1-8. Unity 에디터 레이아웃 확인

Unity 에디터가 열리면 기본 레이아웃을 확인합니다:

```
+---------------------------------------------------+
|                   메뉴 바 (File, Edit, ...)        |
+----------+----------------------+-----------------+
|          |                      |                 |
| Hierarchy|     Scene            |   Inspector     |
|   창     |     (편집 뷰)         |     (속성)      |
|          |                      |                 |
|          |     Game             |                 |
|          |     (플레이 뷰)       |                 |
+----------+----------------------+-----------------+
|                   Project 창                       |
+---------------------------------------------------+
|                   Console 창                       |
+---------------------------------------------------+
```

> 💡 **팁**: 상단 메뉴의 **Window > Layouts**에서 레이아웃을 변경할 수 있습니다.  
> **"Default"** 레이아웃을 사용하는 것을 추천합니다.

---

## 2. Asset Store 예제로 Unity 익히기

> **목적**: Unity 에디터의 기본적인 조작법과 구조를 직접 체험하여 익숙해지는 과정  
> **소요 시간**: 약 30~40분

본격적으로 로봇을 만들기 전에, Unity Asset Store에서 완성된 게임 예제를 다운로드하여 플레이해보면서 Unity의 기본 작동 방식을 체득합니다.

### 2-1. Unity Hub 실행 및 기존 프로젝트 열기

1. **Unity Hub**를 실행합니다.
2. 좌측의 **"Projects"** 탭을 클릭합니다.
3. 아직 프로젝트가 없다면 **"New project"** 버튼을 클릭하여 임시 프로젝트를 하나 만듭니다.
   - 템플릿: **3D Core** 또는 **3D (URP)**
   - 이름: **"UnityExplorer"** (예시)
4. 프로젝트가 열리면 Unity 에디터의 각 영역을 살펴봅니다.

### 2-2. Unity 에디터 레이아웃 이해하기

> 이전 단계(-1-8)에서 이미 레이아웃을 확인했다면, 각 창의 역할을 복습하는 정도로 진행합니다.

Unity 에디터는 다음과 같은 영역으로 구성됩니다:

```
+---------------------------------------------------+
|                     메뉴 바                         |
+----------+----------------------+-----------------+
|          |                      |                 |
| Hierarchy|     Scene / Game     |   Inspector     |
|   창     |        창            |      창         |
|          |                      |                 |
+----------+----------------------+-----------------+
|                   Project 창                       |
+---------------------------------------------------+
|                   Console 창                       |
+---------------------------------------------------+
```

| 영역 | 역할 | 조작법 |
|------|------|--------|
| **Hierarchy** | 현재 씬에 있는 모든 오브젝트 목록 | 오브젝트 선택, 부모-자식 관계 설정 |
| **Scene** | 씬을 3D로 직접 편집하는 뷰 | 마우스 휠: 줌, 우클릭+드래그: 회전, 중간클릭+드래그: 이동 |
| **Game** | 플레이어가 보게 되는 실제 게임 화면 | Play 모드에서만 활성화 |
| **Inspector** | 선택한 오브젝트의 속성(컴포넌트) 편집 | 값 수정, 컴포넌트 추가/제거 |
| **Project** | 프로젝트의 모든 에셋(파일) 관리 | 파일 탐색, 에셋 생성 |
| **Console** | 로그 메시지 및 오류 확인 | Debug.Log 출력 확인 |

> 💡 **팁**: 에디터 레이아웃은 **상단 메뉴 > Window > Layouts**에서 변경할 수 있습니다.

### 2-3. Asset Store 접속

Unity에서 직접 Asset Store를 접속하는 방법과 웹 브라우저로 접속하는 방법이 있습니다.

#### 방법 1: Unity 에디터 내에서 접속

1. Unity 에디터 상단 메뉴에서 **Window > Package Manager**를 엽니다.
2. 좌측 상단 드롭다운에서 **"My Assets"**를 선택합니다.
3. 이곳에서 이전에 다운로드한 에셋을 관리할 수 있습니다.

#### 방법 2: 웹 브라우저로 접속 (추천)

1. 웹 브라우저를 열고 **https://assetstore.unity.com** 에 접속합니다.
2. Unity 계정으로 **로그인**합니다.
   - Unity Hub를 설치했다면 이미 계정이 있을 것입니다.
   - 계정이 없다면 무료로 가입할 수 있습니다.

### 2-4. 무료 3D 게임 예제 검색 및 다운로드

Asset Store에서 초심자가 체험하기 좋은 무료 에셋을 검색합니다.

#### 추천 검색어 및 에셋

Asset Store 검색창에 다음 키워드를 검색해보세요:

| 검색어 | 추천 에셋 | 특징 |
|--------|----------|------|
| `3D starter kit` | Simple 3D Starter | 기본적인 3D 캐릭터 이동 체험 |
| `low poly` | Low Poly Simple Pack | 심플한 3D 모델로 구성된 씬 |
| `FPS kit` | Unity FPS Sample | 완성된 FPS 게임 예제 |
| `platformer 3D` | 3D Platformer Microgame | Unity 공식 3D 플랫포머 |
| `car game` | Karting Microgame | Unity 공식 카트 게임 |

> 🎯 **추천**: Unity에서 제공하는 **공식 마이크로게임**을 사용하는 것을 강력히 추천합니다.
> - Unity Hub > **Learn** 탭에서도 바로 시작할 수 있습니다.

#### 공식 마이크로게임 다운로드 방법

**방법 A: Unity Hub에서 직접 (가장 쉬움)**

1. Unity Hub를 실행합니다.
2. 좌측의 **"Learn"** 탭을 클릭합니다.
3. **"3D Platformer Microgame"** 또는 **"Karting Microgame"**을 찾습니다.
4. **"Download project"** 버튼을 클릭합니다.
5. 다운로드가 완료되면 자동으로 프로젝트가 열립니다.

**방법 B: Asset Store에서 다운로드**

1. https://assetstore.unity.com 에 접속합니다.
2. 검색창에 **"Microgame"** 또는 **"Starter"**를 입력합니다.
3. **Filters > Price > Free**를 선택하여 무료 에셋만 표시합니다.
4. 마음에 드는 에셋을 선택합니다.
5. **"Add to My Assets"** 버튼을 클릭합니다.
6. Unity 에디터로 돌아와서 **Window > Package Manager**를 엽니다.
7. 좌측 상단 드롭다운에서 **"My Assets"**를 선택합니다.
8. 다운로드한 에셋을 찾아 **"Download"** → **"Import"**를 클릭합니다.

### 0-5. 다운로드한 예제 프로젝트 살펴보기

프로젝트가 열리면 다음 항목들을 확인합니다.

#### 1) Hierarchy 창 살펴보기

Hierarchy 창에서 씬의 구조를 파악합니다:

```
Hierarchy 예시 (3D Platformer Microgame)
+--- LEVELS ---
|   +-- Level (1)
|   |   +-- Player
|   |   +-- Platforms
|   |   +-- Enemies
|   |   +-- Collectibles
|   +-- Environment
+--- MANAGERS ---
|   +-- GameManager
|   +-- UIManager
|   +-- AudioManager
+--- ENVIRONMENT ---
|   +-- Ground
|   +-- Trees
|   +-- Skybox
+-- Main Camera
+-- Directional Light
```

- **부모-자식 관계**: 들여쓰기된 오브젝트가 부모 안에 있는 자식 오브젝트
- **--- 표시 ---**: 분류용 빈 오브젝트 (실제로 보이지 않음)

#### 2) Inspector 창에서 컴포넌트 확인

Hierarchy에서 아무 오브젝트나 선택하고 Inspector를 봅니다:

| 컴포넌트 | 역할 | 예시 |
|---------|------|------|
| **Transform** | 위치, 회전, 크기 | 모든 오브젝트에 반드시 있음 |
| **Mesh Renderer** | 3D 모델을 화면에 표시 | 눈에 보이는 부분 |
| **Collider** | 충돌 감지 | 물체가 서로 통과하지 않도록 |
| **Rigidbody** | 물리 시뮬레이션 | 중력, 속도 등 |
| **Animator** | 애니메이션 제어 | 캐릭터 움직임 등 |
| **Script** | 사용자 정의 로직 | 게임 규칙 |

#### 3) Project 창에서 에셋 구조 확인

Project 창을 탐색하며 다음 폴더들을 확인합니다:

```
Assets/
+-- Scenes/        <- 씬 파일 (.unity)
+-- Scripts/       <- C# 스크립트 (.cs)
+-- Prefabs/       <- 프리팹 (재사용 가능한 오브젝트 템플릿)
+-- Materials/     <- 머티리얼 (색상, 질감)
+-- Textures/      <- 텍스처 (이미지)
+-- Models/        <- 3D 모델 (.fbx, .obj 등)
+-- Animations/    <- 애니메이션 클립
+-- Audio/         <- 사운드 파일
+-- Resources/     <- 런타임에 로드되는 에셋
```

### 2-6. Play 모드로 게임 플레이

이제 실제로 게임을 플레이해봅니다.

1. Unity 상단의 **▶ (Play)** 버튼을 클릭합니다.
   - 또는 단축키 **Ctrl + P**
2. **Game 창**이 활성화되면 키보드와 마우스로 게임을 플레이합니다.
3. 플레이 중 관찰할 점:

#### 관찰 포인트

| 항목 | 확인 방법 |
|------|----------|
| **카메라 움직임** | 캐릭터를 따라가는 카메라가 있는지 |
| **물리 효과** | 점프 시 중력이 적용되는지, 물체가 떨어지는지 |
| **충돌** | 벽이나 바닥을 뚫고 지나가지 않는지 |
| **UI** | 체력, 점수 등의 UI가 표시되는지 |
| **이펙트** | 파티클 효과(연기, 불꽃 등)가 있는지 |
| **사운드** | 배경음악이나 효과음이 재생되는지 |

4. 플레이를 멈추려면 **▶ (Play)** 버튼을 다시 클릭합니다.

### 2-7. 간단한 수정 체험

예제 프로젝트를 직접 수정해보면서 Unity의 편집 방식을 체득합니다.

#### 실험 1: 오브젝트 위치 바꾸기

1. Play 모드를 **정지**합니다.
2. Hierarchy에서 캐릭터(예: "Player")를 선택합니다.
3. Inspector의 **Transform > Position** 값을 변경합니다.
4. Play 버튼을 눌러 바뀐 위치에서 시작하는지 확인합니다.
5. **Ctrl + Z**로 실행 취소합니다.

#### 실험 2: 머티리얼(색상) 변경

1. Hierarchy에서 지면이나 벽 오브젝트를 선택합니다.
2. Inspector에서 **Mesh Renderer > Materials**를 찾습니다.
3. Assets 폴더에서 다른 Material을 찾아 드래그합니다.
4. Play 모드에서 색상이 바뀌었는지 확인합니다.

#### 실험 3: 컴포넌트 추가/제거

1. 아무 오브젝트나 선택합니다.
2. Inspector 하단의 **"Add Component"** 버튼을 클릭합니다.
3. **"Rigidbody"**를 검색하여 추가합니다.
4. Play 버튼을 누르면 해당 오브젝트에 중력이 적용되어 떨어집니다.
5. Rigidbody 컴포넌트의 **"..."** 메뉴에서 **"Remove Component"**로 제거합니다.

#### 실험 4: 오브젝트 복제

1. Hierarchy에서 아무 오브젝트나 선택합니다.
2. **Ctrl + D**를 눌러 복제합니다.
3. 복제된 오브젝트의 위치를 변경하여 씬에 추가합니다.
4. Play 모드에서 복제된 오브젝트도 같은 동작을 하는지 확인합니다.

### 2-8. 핵심 개념 정리

Unity를 사용하기 전에 반드시 이해해야 할 핵심 개념들을 정리합니다.

| 개념 | 설명 | 비유 |
|------|------|------|
| **Scene** | 게임의 하나의 화면/무대 | 연극의 한 막 |
| **GameObject** | 씬에 존재하는 모든 오브젝트 | 무대 위의 배우, 소품 |
| **Component** | 오브젝트에 붙이는 기능 모듈 | 배우의 능력 (연기, 대사 등) |
| **Transform** | 위치/회전/크기를 담당하는 필수 컴포넌트 | 무대 위 좌석 번호 |
| **Prefab** | 재사용 가능한 오브젝트 템플릿 | 배역별 의상/소품 세트 |
| **Material** | 오브젝트의 색상과 질감 | 의상의 색상과 무늬 |
| **Script** | C# 코드로 게임 로직을 구현 | 연극 대본 |
| **Asset Store** | Unity 에셋을 사고파는 마켓플레이스 | 앱스토어 같은 곳 |

### 2-9. 다음 단계로 넘어가기

위의 예제 프로젝트를 체험하면서 Unity에 대한 감을 잡았다면, 이제 직접 로봇을 만들어보는 메인 튜토리얼로 넘어갑니다.

> 💡 **팁**: 예제 프로젝트를 그대로 두어도 좋고, 새로 만든 프로젝트에서 시작해도 됩니다.  
> 새로운 프로젝트를 만들려면 **File > New Project** 또는 Unity Hub에서 새로 만들면 됩니다.

---

## 3. 프로젝트 생성 및 기본 설정

### 3-1. Unity Hub에서 새 프로젝트 만들기

1. **Unity Hub**를 실행합니다.
2. 좌측 상단의 **"New project"** 버튼을 클릭합니다.
3. 템플릿에서 **"3D (URP)"** 또는 **"3D Core"**를 선택합니다.
   - URP(Universal Render Pipeline)를 추천합니다. 렌더링 품질이 좋고 성능도 우수합니다.
4. 프로젝트 이름을 **"RobotTutorial"**로 입력합니다.
5. 프로젝트 저장 위치를 설정한 뒤 **"Create project"** 버튼을 클릭합니다.

### 3-2. 씬(Scene) 확인

1. 프로젝트가 열리면 기본 씬이 로드됩니다.
2. Unity 상단 메뉴에서 **File > New Scene**을 클릭하여 새로운 씬을 만듭니다.
3. **File > Save As**로 씬 이름을 **"RobotScene"**으로 저장합니다.

### 3-3. 씬 구조 이해하기

Hierarchy 창(좌측)에는 현재 씬에 있는 모든 게임 오브젝트가 나열됩니다. 기본적으로 다음 오브젝트들이 있습니다:

| 오브젝트 | 역할 |
|---------|------|
| **Main Camera** | 게임 화면을 렌더링하는 카메라 |
| **Directional Light** | 장면 전체를 비추는 태양빛 같은 광원 |

> 💡 **팁**: Inspector 창(우측)에서 선택한 오브젝트의 속성을 확인하고 수정할 수 있습니다.

---

## 4. 지면(Ground) 만들기

로봇이 서 있을 바닥이 필요합니다.

### 4-1. Plane 생성

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Plane**을 클릭합니다.
3. 생성된 Plane의 이름을 **"Ground"**로 변경합니다.
   - 이름을 바꾸려면 Hierarchy에서 해당 오브젝트를 선택한 뒤, Inspector 상단의 이름 필드를 수정하거나, 오브젝트를 선택한 상태에서 **F2** 키를 누릅니다.

### 4-2. Plane 크기 설정

1. Hierarchy에서 **Ground**를 선택합니다.
2. Inspector 창에서 **Transform** 컴포넌트를 찾습니다.
3. **Scale** 값을 **X: 5, Y: 1, Z: 5**로 설정합니다.
   - 이렇게 하면 바닥이 충분히 넓어져서 로봇이 움직여도 떨어지지 않습니다.

### 4-3. Ground 색상 설정 (선택사항)

Ground 색상을 나중에 Material로 변경할 수 있지만, 일단 놔둡니다. 나중에 8절에서 색상을 지정할 때 함께 진행합니다.

---

## 5. 로봇 몸통(Cube) 만들기

Isaac Sim 튜토리얼에서는 **Create > Shape > Cube**로 상자를 만들고 Z축 위치와 Scale을 변경했습니다. Unity에서는 다음과 같이 진행합니다.

### 5-1. Cube 생성

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Cube**를 클릭합니다.
3. 생성된 Cube의 이름을 **"Body"**로 변경합니다.

### 5-2. Transform 설정

1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector 창의 **Transform** 컴포넌트에서 값을 설정합니다.

| 속성 | Isaac Sim 값 | Unity 대응 | Unity 값 |
|------|-------------|-----------|---------|
| Position | (0, 0, 1) | Position Y (Unity에서는 Y축이 위) | **(0, 1, 0)** |
| Scale | (1, 2, 0.5) | Scale | **(1, 2, 0.5)** |

> ⚠️ **중요 차이점**: Isaac Sim에서는 Z축이 위쪽이지만, Unity에서는 **Y축이 위쪽**입니다. 따라서 Isaac Sim의 Translate Z=1은 Unity의 Position Y=1에 해당합니다.

#### 상세 설정 방법

**Position:**
- X: `0`
- Y: `1` (지면 위에 떠 있도록)
- Z: `0`

**Scale:**
- X: `1`
- Y: `2`
- Z: `0.5`

### 5-3. 결과 확인

Scene 창에서 Body가 지면 위에 떠 있는 것을 확인할 수 있습니다. 카메라 조작법:
- **마우스 휠**: 줌 인/아웃
- **마우스 우클릭 + 드래그**: 카메라 회전
- **마우스 휠 클릭 + 드래그**: 카메라 팬(이동)

---

## 6. 로봇 바퀴(Cylinder) 만들기

Isaac Sim에서는 **Create > Shape > Cylinder**를 사용했습니다. Unity에서도 동일하게 Cylinder를 사용합니다.

### 6-1. 첫 번째 바퀴 생성

1. Hierarchy 창의 빈 공간을 **우클릭**합니다.
2. **3D Object > Cylinder**를 클릭합니다.
3. 생성된 Cylinder의 이름을 **"Wheel_Right"**로 변경합니다.

### 6-2. 첫 번째 바퀴 Transform 설정

Isaac Sim에서는:
- Translate: (1.5, 0, 1.0)
- Orient Y: 90도

Unity에서는 다음과 같이 설정합니다:

**Position:**
- X: `1.5` (몸통 오른쪽)
- Y: `0.5` (바닥에 닿도록 낮춤)
- Z: `0`

**Rotation:**
- X: `0`
- Y: `0`
- Z: `90` (Isaac Sim의 Orient Y 90도 = Unity의 Z축 90도 회전)

**Scale:**
- X: `1`
- Y: `0.5` (Isaac Sim의 Radius 0.5에 대응)
- Z: `1`

> 💡 **팁**: Unity의 기본 Cylinder는 Y축을 기준으로 세워져 있으므로, Z축으로 90도 회전해야 Isaac Sim과 같은 방향(옆으로 눕힌 형태)이 됩니다.

### 6-3. 두 번째 바퀴 생성 (복제)

1. Hierarchy에서 **Wheel_Right**을 선택합니다.
2. **Ctrl + D** 키를 눌러 복제합니다.
3. 복제된 오브젝트의 이름을 **"Wheel_Left"**로 변경합니다.
4. Inspector의 Transform에서 **Position X**를 **`-1.5`**로 변경합니다.

> Isaac Sim에서는 복제 후 Translate X를 -1.5로 변경했습니다. Unity도 동일합니다.

### 4-4. 최종 로봇 구조 확인

Hierarchy 창에 다음과 같은 구조가 보여야 합니다:

```
RobotScene
├── Main Camera
├── Directional Light
├── Ground
├── Body           ← 로봇 몸통 (Cube)
├── Wheel_Right    ← 오른쪽 바퀴 (Cylinder)
└── Wheel_Left     ← 왼쪽 바퀴 (Cylinder)
```

> 💡 **팁**: 모든 로봇 파트를 하나의 빈 오브젝트 아래에 자식으로 넣으면 관리가 편해집니다.
> - Hierarchy 빈 공간 우클릭 > **Create Empty** > 이름을 **"Robot"**으로 변경
> - Body, Wheel_Right, Wheel_Left을 모두 선택한 뒤 Robot 오브젝트로 드래그하여 자식으로 만듦

---

## 7. 물리 효과 적용 (Rigidbody 및 Collider)

이제 Isaac Sim에서 했던 것처럼 물리 효과를 적용합니다. 시뮬레이션을 돌리면 객체가 중력에 의해 떨어지도록 만들어야 합니다.

### 7-1.理解하기: Unity의 물리 시스템

Unity의 물리 시스템은 두 가지 핵심 컴포넌트로 구성됩니다:

| 컴포넌트 | 역할 |
|---------|------|
| **Rigidbody** | 물리 엔진에 의해 움직이는 오브젝트에 추가. 중력, 힘, 질량 등을 다룸 |
| **Collider** | 충돌 감지용 형태. 오브젝트가 서로 통과하지 못하게 함 |

> Isaac Sim의 "Rigid Body with Colliders Preset" = Unity의 **Rigidbody + Collider** 조합

### 7-2. Body에 Rigidbody 및 Box Collider 추가

1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector 창 하단의 **"Add Component"** 버튼을 클릭합니다.
3. **Rigidbody**를 검색하여 선택합니다.
4. 다시 **Add Component**를 클릭합니다.
5. **Box Collider**를 검색하여 선택합니다.
   - Unity의 Cube에는 기본적으로 Box Collider가 자동으로 추가되어 있을 수 있습니다.

#### Rigidbody 설정값

Inspector에서 Rigidbody 컴포넌트의 값을 확인합니다:

| 속성 | 값 | 설명 |
|------|---|------|
| Mass | `1` | 질량 (기본값, 필요시 조정) |
| Drag | `0` | 공기 저항 (0 = 없음) |
| Angular Drag | `0.05` | 회전 저항 |
| Use Gravity | ✅ 체크 | 중력 적용 |
| Is Kinematic | ❌ 체크 해제 | 물리 엔진에 의해 움직임 |

### 7-3. 바퀴에 Rigidbody 및 Capsule Collider 추가

1. Hierarchy에서 **Wheel_Right**을 선택합니다.
2. **Add Component > Rigidbody**를 추가합니다.
3. **Add Component > Capsule Collider**를 추가합니다.
   - Capsule Collider는 원통에 더 적합합니다.

#### Capsule Collider 설정

| 속성 | 값 | 설명 |
|------|---|------|
| Center | (0, 0, 0) | 중심점 (기본값) |
| Radius | `0.5` | 반지름 |
| Height | `1` | 높이 |
| Direction | **Z-Axis** | 캡슐의 방향을 Z축으로 설정 (옆으로 눕힌 원통에 맞춤) |

4. 같은 방법으로 **Wheel_Left**에도 Rigidbody와 Capsule Collider를 추가합니다.

### 7-4. Ground에 Collider 추가

1. Hierarchy에서 **Ground**를 선택합니다.
2. **Add Component > Mesh Collider**를 추가합니다.
   - Plane에는 Mesh Collider가 자동으로 추가되어 있을 수 있습니다.
3. Rigidbody는 **추가하지 않습니다**. (바닥은 움직이면 안 됨)

### 7-5. 시뮬레이션 테스트

1. Unity 상단의 **▶ (Play)** 버튼을 클릭합니다.
2. 로봇이 중력에 의해 떨어지는 것을 확인합니다.
3. **▶ (Play)** 버튼을 다시 클릭하여 정지합니다.

> ⚠️ **문제 해결**: 로봇이 떨어지지 않으면 다음을 확인하세요:
> - Rigidbody의 **Use Gravity**가 체크되어 있는지
> - Ground의 Collider가 있는지
> - Body의 Y 위치가 0보다 큰지 (지면 위에 있는지)

---

## 8. 충돌 검사 윤곽선 확인

Isaac Sim에서는 **Show By Type > Physics > Colliders > All**로 윤곽선을 표시했습니다. Unity에서는 다음 방법을 사용합니다.

### 8-1. Scene 뷰에서 충돌체 표시

1. Scene 창 상단의 **Gizmos** 드롭다운 메뉴를 클릭합니다.
2. **"Gizmos"** 버튼이 활성화(파란색)되어 있는지 확인합니다.

### 8-2. 전체 충돌체 윤곽선 표시 (Scene 뷰)

Unity의 Scene 뷰에서는 모든 Collider가 **초록색 와이어프레임**으로 자동 표시됩니다.

만약 표시되지 않는다면:
1. Scene 창 상단의 **Draw Mode**를 **Shaded**에서 **Wireframe**으로 변경해 보세요.
2. 또는 Scene 뷰의 **Gizmos** 버튼을 클릭하여 활성화하세요.

### 8-3. Play 모드에서 충돌체 표시

1. Unity 상단 메뉴에서 **Edit > Project Settings**를 엽니다.
2. 좌측에서 **Physics**를 선택합니다.
3. 우측 하단의 **"Layer Collision Matrix"** 아래에 **"Queries Hit Triggers"** 옵션을 확인합니다.

### 8-4. 커스텀 충돌체 시각화 (선택)

더 명확하게 충돌체를 보고 싶다면 스크립트를 사용할 수 있습니다:

1. Project 창에서 우클릭 > **Create > C# Script** > 이름을 **"ShowColliders"**로 변경
2. 스크립트를 더블클릭하여 Visual Studio 또는 Rider에서 엽니다.
3. 다음 코드를 입력합니다:

```csharp
using UnityEngine;

public class ShowColliders : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // 모든 Collider의 윤곽선을 그립니다
        foreach (var collider in FindObjectsOfType<Collider>())
        {
            Gizmos.color = Color.green;

            if (collider is BoxCollider box)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (collider is CapsuleCollider capsule)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
            }
            else if (collider is SphereCollider sphere)
            {
                Gizmos.matrix = collider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}
```

4. 저장하고 Unity로 돌아옵니다.
5. 이 스크립트를 **任何** 빈 오브젝트(예: "GameManager")에 붙입니다.

---

## 9. 접촉 및 마찰 매개변수 (Physic Material)

Isaac Sim에서는 **Create > Physics > Physics Material**로 마찰력과 탄성을 조절했습니다. Unity에서는 **Physic Material** (유니티에서는 "Physic"이 정확한 명칭)을 사용합니다.

### 9-1. Physic Material 생성

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > Physic Material**을 클릭합니다.
3. 이름을 **"RobotMaterial"**로 변경합니다.

> ⚠️ 주의: "Physic Material"은 "Physic**s** Material"과 다릅니다.
> - **Physic Material**: 3D 물리용 (Collider에 적용)
> - **Physics Material 2D**: 2D 물리용

### 9-2. 매개변수 설정

RobotMaterial을 선택하고 Inspector에서 다음 값을 설정합니다:

| 속성 | 값 | 설명 |
|------|---|------|
| Dynamic Friction | `0.6` | 움직이는 물체의 마찰력 (0~1) |
| Static Friction | `0.6` | 멈춰있는 물체의 마찰력 (0~1) |
| Bounciness | `0.1` | 탄성 (0=안 튕김, 1=완전 반사) |
| Friction Combine | **Multiply** | 마찰력 결합 방식 |
| Bounce Combine | **Average** | 탄성 결합 방식 |

> 💡 **팁**: Isaac Sim의 접촉 매개변수와 유사하게 설정할 수 있습니다.
> - 마찰력이 높을수록 바퀴가 미끄러지지 않음
> - 탄성이 높을수록 강하게 튕겨옴

### 9-3. Collider에 Physic Material 적용

**Body (Box Collider)에 적용:**
1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector에서 **Box Collider** 컴포넌트를 찾습니다.
3. **Material** 필드에 **RobotMaterial**을 드래그하거나 클릭하여 선택합니다.

**바퀴 (Capsule Collider)에 적용:**
1. **Wheel_Right**과 **Wheel_Left**을 각각 선택합니다.
2. 각각의 **Capsule Collider**의 **Material** 필드에 **RobotMaterial**을 적용합니다.

### 9-4. Ground용 별도 Material (선택)

지면의 마찰력을 별도로 설정하고 싶다면:

1. Project 창에서 우클릭 > **Create > Physic Material** > 이름을 **"GroundMaterial"**로 변경
2. 설정값:

| 속성 | 값 |
|------|---|
| Dynamic Friction | `0.8` |
| Static Friction | `0.8` |
| Bounciness | `0` |

3. Ground의 Mesh Collider에 적용합니다.

---

## 10. 객체의 색상 변경 (Material)

Isaac Sim에서는 **Create > Material > OmniPBR**을 사용하여 Body와 Wheel의 색상을 변경했습니다. Unity에서는 **Material**과 **Shader**를 사용합니다.

### 10-1. URP용 Material 생성 (URP 프로젝트인 경우)

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > Material**을 클릭합니다.
3. 이름을 **"BodyMaterial"**로 변경합니다.
4. Inspector 상단의 **Shader** 드롭다운에서 **Universal Render Pipeline > Lit**을 선택합니다.

### 10-1-2. Built-in Render Pipeline용 Material (3D Core 프로젝트인 경우)

1. Project 창에서 우클릭합니다.
2. **Create > Material**을 클릭합니다.
3. 이름을 **"BodyMaterial"**로 변경합니다.
4. Inspector 상단의 **Shader**에서 **Standard**가 선택되어 있는지 확인합니다.

### 10-2. BodyMaterial 색상 설정

1. **BodyMaterial**을 선택합니다.
2. Inspector에서 **Albedo** 옆의 색상 블록을 클릭합니다.
3. 색상 선택기에서 **파란색 (R: 50, G: 100, B: 200)** 정도를 선택합니다.
4. Metallic 값을 `0.3`, Smoothness 값을 `0.7` 정도로 조정합니다.

### 10-3. WheelMaterial 생성 및 설정

1.同样的 방법으로 새 Material을 만듭니다.
2. 이름을 **"WheelMaterial"**로 변경합니다.
3. **Albedo** 색상을 **검은색 (R: 30, G: 30, B: 30)** 으로 설정합니다.
4. Metallic: `0.5`, Smoothness: `0.3`으로 설정합니다.

### 10-4. GroundMaterial 생성 및 설정

1. 새 Material을 만들고 이름을 **"GroundMaterial"**로 합니다.
2. **Albedo** 색상을 **밝은 회색 (R: 200, G: 200, B: 200)** 으로 설정합니다.

### 10-5. Material 적용

**Body에 적용:**
1. Hierarchy에서 **Body**를 선택합니다.
2. Inspector에서 **Mesh Renderer** 컴포넌트를 찾습니다.
3. **Materials > Element 0** 필드에 **BodyMaterial**을 드래그하거나 클릭하여 선택합니다.

**바퀴에 적용:**
1. **Wheel_Right**과 **Wheel_Left**을 선택합니다.
2. 각각의 Mesh Renderer의 **Element 0**에 **WheelMaterial**을 적용합니다.

**Ground에 적용:**
1. **Ground**를 선택합니다.
2. Mesh Renderer의 **Element 0**에 **GroundMaterial**을 적용합니다.

### 10-6. 최종 구조

```
Assets/
├── Materials/
│   ├── BodyMaterial       ← 파란색 (몸통)
│   ├── WheelMaterial      ← 검은색 (바퀴)
│   └── GroundMaterial     ← 밝은 회색 (바닥)
├── PhysicMaterials/
│   └── RobotMaterial      ← 마찰/탄성 설정
└── Scenes/
    └── RobotScene.unity
```

---

## 11. 키보드 입력으로 로봇 조종하기

이제 로봇을 키보드로 조종하는 기능을 추가합니다. **W, A, S, D, X** 또는 **화살표 키**로 이동하고, **스페이스바**로 멈추는 기능입니다.

### 11-1. Robot 빈 오브젝트 만들기

모든 로봇 파트를 하나의 부모 오브젝트로 묶습니다.

1. Hierarchy에서 빈 공간을 **우클릭** > **Create Empty**를 클릭합니다.
2. 이름을 **"Robot"**으로 변경합니다.
3. **Body**, **Wheel_Right**, **Wheel_Left**을 모두 선택합니다.
4. 선택된 오브젝트들을 **Robot** 오브젝트 위로 **드래그**하여 자식으로 만듭니다.

### 11-2. RobotController 스크립트 만들기

1. Project 창의 **Assets** 폴더에서 우클릭합니다.
2. **Create > C# Script**를 클릭합니다.
3. 이름을 **"RobotController"**로 변경합니다.
4. 스크립트를 **더블클릭**하여 Visual Studio 또는 Visual Studio Code에서 엽니다.

### 11-3. 스크립트 코드 작성

다음 코드를 기존 코드에 **전체 교체**합니다:

```csharp
using UnityEngine;

public class RobotController : MonoBehaviour
{
    // ===== 이동 설정 =====
    [Header("이동 설정")]
    [Tooltip("이동 속도 (m/s)")]
    public float moveSpeed = 5.0f;

    [Tooltip("회전 속도 (degrees/s)")]
    public float rotationSpeed = 120.0f;

    [Tooltip("점프 힘")]
    public float jumpForce = 7.0f;

    // ===== 내부 변수 =====
    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("RobotController: Rigidbody 컴포넌트가 없습니다! Robot 오브젝트에 Rigidbody를 추가해주세요.");
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveRobot();
    }

    /// <summary>
    /// 키보드 입력을 처리합니다.
    /// </summary>
    void HandleInput()
    {
        // 스페이스바로 멈춤
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopRobot();
        }
    }

    /// <summary>
    /// 로봇을 이동시킵니다. FixedUpdate에서 매 프레임 호출됩니다.
    /// </summary>
    void MoveRobot()
    {
        if (rb == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        // ===== W, A, S, D 입력 =====
        // W 또는 UpArrow: 전진
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        // S 또는 DownArrow: 후진
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }
        // A 또는 LeftArrow: 좌회전
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        // D 또는 RightArrow: 우회전
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }

        // ===== X 키: 후진 (선택 기능) =====
        if (Input.GetKey(KeyCode.X))
        {
            moveZ = -1f;
        }

        // ===== 이동 적용 =====
        // 전진/후진: 로봇의 앞방향으로 이동
        Vector3 moveDirection = transform.forward * moveZ;
        moveDirection = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // 좌/우 회전
        float rotation = moveX * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    /// <summary>
    /// 로봇을 즉시 멈춥니다. 스페이스바로 호출됩니다.
    /// </summary>
    void StopRobot()
    {
        if (rb == null) return;

        // 속도를 0으로 만들어 완전히 멈춤
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("로봇이 멈췄습니다!");
    }

    /// <summary>
    /// 바닥과 접촉했는지 확인합니다. (점프를 위해 사용)
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
```

> ⚠️ **중요**: Unity 2023 이상 버전에서는 `Rigidbody.velocity`가 `Rigidbody.linearVelocity`로 변경되었습니다. 본인의 Unity 버전에 맞게 수정해주세요.
> - Unity 2022 이하: `rb.velocity`, `rb.angularVelocity`
> - Unity 2023 이상: `rb.linearVelocity`, `rb.angularVelocity`

### 11-4. 스크립트를 Robot에 적용

1. Unity로 돌아옵니다.
2. Hierarchy에서 **Robot** 오브젝트를 선택합니다.
3. Inspector 하단의 **"Add Component"** 버튼을 클릭합니다.
4. **RobotController**를 검색하여 선택합니다.
5. Inspector에서 **RobotController** 컴포넌트의 값을 확인합니다:

| 속성 | 기본값 | 설명 |
|------|-------|------|
| Move Speed | `5` | 이동 속도 |
| Rotation Speed | `120` | 회전 속도 |
| Jump Force | `7` | 점프 힘 (현재 사용 안 함) |

### 11-5. Rigidbody 추가 (Robot 부모 오브젝트)

Robot 부모 오브젝트에도 Rigidbody가 있어야 합니다.

1. **Robot** 오브젝트를 선택합니다.
2. **Add Component > Rigidbody**를 추가합니다.
3. Rigidbody 설정을 확인합니다:

| 속성 | 값 |
|------|---|
| Mass | `1` |
| Use Gravity | ✅ 체크 |
| Is Kinematic | ✅ **체크** (부모 오브젝트는 직접 물리 적용 안 함) |

> 💡 **팁**: 부모 오브젝트의 Rigidbody를 Kinematic으로 설정하면, 자식 오브젝트(바퀴)들이 물리적으로 자유롭게 움직이면서도 부모를 따라갑니다.

### 11-6. Ground에 태그(Tag) 설정

점프 기능을 위해 바닥에 "Ground" 태그를 지정합니다.

1. Hierarchy에서 **Ground**를 선택합니다.
2. Inspector 상단의 **Tag** 드롭다운을 클릭합니다.
3. **"Add Tag..."**를 클릭합니다.
4. Tags 목록에서 **"+"** 버튼을 클릭합니다.
5. 이름을 **"Ground"**로 입력하고 **Save**를 클릭합니다.
6. 다시 Hierarchy에서 **Ground**를 선택하고, Inspector의 **Tag** 드롭다운에서 **"Ground"**를 선택합니다.

### 11-7. 키보드 조작법 요약

| 키 | 동작 |
|----|------|
| **W** 또는 **↑** | 전진 (로봇 앞쪽으로 이동) |
| **S** 또는 **↓** | 후진 (로봇 뒤쪽으로 이동) |
| **A** 또는 **←** | 좌회전 |
| **D** 또는 **→** | 우회전 |
| **X** | 후진 (S와 동일) |
| **스페이스바** | 즉시 멈춤 (속도 0으로 초기화) |

### 11-8. Play 버튼으로 테스트

1. Unity 상단의 **▶ (Play)** 버튼을 클릭합니다.
2. Game 창에서 키보드를 눌러 로봇을 조종합니다.
3. **스페이스바**를 눌러 로봇이 멈추는지 확인합니다.
4. **▶ (Play)** 버튼을 다시 클릭하여 정지합니다.

---

## 12. 최종 테스트 및 정리

### 12-1. 전체 기능 테스트 체크리스트

Play 모드에서 다음 항목을 모두 확인합니다:

- [ ] 로봇이 중력에 의해 지면 위에 올바르게 서 있는지
- [ ] W키/↑로 로봇이 전진하는지
- [ ] S키/↓로 로봇이 후진하는지
- [ ] A키/←으로 로봇이 좌회전하는지
- [ ] D키/→으로 로봇이 우회전하는지
- [ ] X키로 로봇이 후진하는지
- [ ] 스페이스바로 로봇이 즉시 멈추는지
- [ ] 로봇이 지면을 뚫고 지나가지 않는지
- [ ] 로봇의 색상이 올바르게 표시되는지

### 12-2. 최종 씬 구조

```
RobotScene
├── Main Camera
├── Directional Light
├── Ground              ← Mesh Collider + GroundMaterial
├── Robot               ← Rigidbody(Kinematic) + RobotController
│   ├── Body            ← Rigidbody + Box Collider + BodyMaterial
│   ├── Wheel_Right     ← Rigidbody + Capsule Collider + WheelMaterial
│   └── Wheel_Left      ← Rigidbody + Capsule Collider + WheelMaterial
```

### 12-3. 최종 Assets 구조

```
Assets/
├── Materials/
│   ├── BodyMaterial       (파란색)
│   ├── WheelMaterial      (검은색)
│   └── GroundMaterial     (밝은 회색)
├── PhysicMaterials/
│   └── RobotMaterial      (마찰/탄성)
├── Scripts/
│   └── RobotController.cs (키보드 입력 처리)
└── Scenes/
    └── RobotScene.unity
```

---

## Isaac Sim vs Unity 비교표

| 항목 | Isaac Sim | Unity |
|------|----------|-------|
| 오브젝트 생성 | Create > Shape > ... | Hierarchy 우클릭 > 3D Object > ... |
| 좌표축 (위) | Z축 | Y축 |
| 물리 바디 | Physics > Rigid Body with Colliders | Add Component > Rigidbody + Collider |
| 물리 재질 | Create > Physics > Physics Material | Create > Physic Material |
| 머티리얼 | Create > Material > OmniPBR | Create > Material |
| 재질 적용 | Property > Materials | Mesh Renderer > Materials |
| 충돌체 표시 | Show By Type > Physics > Colliders | Scene 뷰 Gizmos |
| 시뮬레이션 | Play 버튼 (상단) | ▶ Play 버튼 (상단) |

---

## 문제 해결 (FAQ)

### Q1: 로봇이 움직이지 않아요
- **RobotController** 스크립트가 **Robot** 오브젝트에 붙어있는지 확인
- Rigidbody의 **Is Kinematic**이 체크되어 있는지 확인
- Play 모드에서 Game 창이 활성화되어 있는지 확인 (키보드 입력은 Game 창에서만 작동)

### Q2: 로봇이 땅을 뚫고 지나가요
- Ground의 Collider가 있는지 확인
- 바퀴/몸통의 Collider와 Ground의 Collider가 서로 다른 Layer에 있는지 확인

### Q3: 로봇이 너무 빠르거나 느려요
- RobotController의 **Move Speed** 값을 조정합니다 (기본값: 5)

### Q4: rb.linearVelocity 오류가 나요
- Unity 버전이 2022 이하라면 `rb.linearVelocity`를 `rb.velocity`로 변경하세요.

### Q5: 키보드 입력이 안 먹혀요
- Game 창을 **마우스로 클릭**하여 포커스를 맞춘 뒤 키보드를 누르세요.
- 다른 UI 오브젝트가 입력을 가로채고 있는지 확인하세요.

---

## 확장 아이디어

튜토리얼을 완료했다면, 다음 기능들을 추가로 도전해보세요:

1. **스페이스바 점프**: isGrounded 체크 후 위쪽으로 힘 주기
2. **마우스 회전**: 마우스 좌우 움직임으로 카메라가 로봇을 따라가도록
3. **가속/감속**: 키를 누르고 있으면 점점 빨라지고, 놓으면 천천히 멈추도록
4. **부스터**: Shift 키를 누르면 이동 속도 2배
5. **바퀴 회전 애니메이션**: 이동할 때 바퀴가 실제로 굴러가는 시각 효과
6. **Python에서 센서 데이터 전송**: Unity의 로봇 위치를 Python으로 실시간 전송
7. **이중 통신**: Unity에서 Python으로 상태를 보내고, Python에서 제어 명령을 받는 양방향 통신
8. **여러 로봇 제어**: Python에서 여러 대의 로봇을 동시에 제어

---

## 13. 외부 Python 프로그램과 연결하기

> **목적**: Unity에서 만든 로봇을 외부 Python 프로그램에서 키보드 입력으로 원격 제어  
> **소요 시간**: 약 40~60분  
> **전제 조건**: 9절(키보드 입력으로 로봇 조종하기)을 완료해야 합니다

### 13-1. 개요: 왜 외부 연결이 필요한가?

지금까지는 Unity 안에서 직접 키보드를 눌러 로봇을 조종했습니다. 하지만 실제 로봇 공학이나 자율주행 시뮬레이션에서는 다음과 같은 이유로 외부 프로그램과의 연결이 필요합니다:

| 이유 | 설명 |
|------|------|
| **로봇 제어 알고리즘** | Python으로 만든 AI/ML 모델이 로봇을 제어해야 할 때 |
| **센서 데이터 처리** | 외부 센서에서 받은 데이터를 기반으로 로봇을 움직일 때 |
| **데이터 수집** | 로봇의 상태를 Python에서 기록/분석할 때 |
| **멀티 에이전트** | 여러 로봇을 하나의 Python 프로그램에서 동시에 제어할 때 |

### 11-2. 연결 방식 선택: TCP/IP 소켓 통신

Unity와 Python을 연결하는 방법은 여러 가지가 있지만, 초심자에게 가장 이해하기 쉬운 방식인 **TCP/IP 소켓 통신**을 사용합니다.

```
┌──────────────────┐         TCP/IP          ┌──────────────────┐
│                  │    (localhost:5000)      │                  │
│   Python 클라이언트  │ <--------------------> │   Unity 서버     │
│                  │                          │                  │
│  - 키보드 입력    │    키 값 전송             │  - 로봇 제어     │
│  - 데이터 처리    │    "W", "A", "S" 등      │  - 상태 수신     │
│                  │                          │                  │
└──────────────────┘                          └──────────────────┘
```

**동작 흐름:**
1. Unity에서 TCP 서버를 실행 (포트 5000)
2. Python에서 Unity 서버에 연결 (클라이언트)
3. Python에서 키보드 입력을 감지
4. 입력된 키 값을 문자열로 전송 (예: "W", "A", "SPACE")
5. Unity에서 값을 수신하여 로봇을 움직임

### 13-3. Unity 측: TCP 서버 스크립트 만들기

#### 1) 새 스크립트 생성

1. Project 창의 **Assets > Scripts** 폴더를 엽니다.
2. 우클릭 > **Create > C# Script** > 이름을 **"TCPServer"**로 변경합니다.
3. 스크립트를 더블클릭하여 Visual Studio에서 엽니다.

#### 2) TCPServer.cs 코드 작성

다음 코드를 기존 코드에 **전체 교체**합니다:

```csharp
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class TCPServer : MonoBehaviour
{
    [Header("서버 설정")]
    [Tooltip("리스닝할 포트 번호")]
    public int port = 5000;

    [Tooltip("연결된 클라이언트 수 표시")]
    public int connectedClients = 0;

    // TCP 서버
    private TcpListener server;
    private Thread serverThread;
    private bool isRunning = false;

    // 수신된 명령 큐 (메인 스레드에서 처리)
    private readonly Queue<string> commandQueue = new Queue<string>();
    private readonly object queueLock = new object();

    // 수신된 마지막 명령
    [HideInInspector]
    public string lastCommand = "";

    void Start()
    {
        StartServer();
    }

    void OnDestroy()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    /// <summary>
    /// TCP 서버를 시작합니다.
    /// </summary>
    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            isRunning = true;

            // 백그라운드 스레드에서 클라이언트 연결 대기
            serverThread = new Thread(ServerLoop);
            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log($"[TCPServer] 서버 시작됨 - 포트: {port}");
            Debug.Log($"[TCPServer] Python에서 연결 대기 중... (localhost:{port})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TCPServer] 서버 시작 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 서버 메인 루프 (백그라운드 스레드)
    /// </summary>
    void ServerLoop()
    {
        while (isRunning)
        {
            try
            {
                // 클라이언트 연결 대기
                TcpClient client = server.AcceptTcpClient();
                connectedClients++;
                Debug.Log($"[TCPServer] 클라이언트 연결됨! (현재 {connectedClients}개)");

                // 각 클라이언트를 위한 수신 스레드 생성
                Thread receiveThread = new Thread(() => HandleClient(client));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (SocketException)
            {
                // 서버가 중지되면 발생
                break;
            }
        }
    }

    /// <summary>
    /// 클라이언트로부터 데이터를 수신합니다.
    /// </summary>
    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected && isRunning)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                
                // 줄바꿈으로 구분된 여러 명령 처리
                string[] commands = message.Split('\n');
                foreach (string cmd in commands)
                {
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        lock (queueLock)
                        {
                            commandQueue.Enqueue(cmd);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"[TCPServer] 클라이언트 처리 중 오류: {e.Message}");
        }
        finally
        {
            client.Close();
            connectedClients--;
            Debug.Log($"[TCPServer] 클라이언트 연결 해제 (현재 {connectedClients}개)");
        }
    }

    /// <summary>
    /// 메인 스레드에서 명령을 처리합니다.
    /// </summary>
    void Update()
    {
        lock (queueLock)
        {
            while (commandQueue.Count > 0)
            {
                string command = commandQueue.Dequeue();
                ProcessCommand(command);
            }
        }
    }

    /// <summary>
    /// 수신된 명령을 처리합니다.
    /// </summary>
    void ProcessCommand(string command)
    {
        lastCommand = command;
        Debug.Log($"[TCPServer] 수신 명령: {command}");
    }

    /// <summary>
    /// 클라이언트에게 메시지를 전송합니다.
    /// </summary>
    public void SendToClient(string message)
    // 메시지 전송은 별도 구현 필요 (생략)
    {
        // 실제 구현에서는 연결된 클라이언트 목록을 관리해야 합니다
    }

    /// <summary>
    /// 서버를 중지합니다.
    /// </summary>
    void StopServer()
    {
        isRunning = false;
        server?.Stop();
        serverThread?.Join(1000);
        Debug.Log("[TCPServer] 서버 중지됨");
    }
}
```

#### 3) RobotController 스크립트 수정

기존의 **RobotController.cs** 스크립트를 열고, TCPServer와 연동하도록 수정합니다.

**다음 코드를 기존 RobotController.cs에 전체 교체합니다:**

```csharp
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 120.0f;
    public float jumpForce = 7.0f;

    [Header("연결 설정")]
    [Tooltip("TCPServer 컴포넌트를 연결하세요")]
    public TCPServer tcpServer;

    private Rigidbody rb;
    private bool isGrounded;

    // 외부 제어용 입력 값
    private float externalMoveZ = 0f;
    private float externalMoveX = 0f;
    private bool externalStop = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("RobotController: Rigidbody 컴포넌트가 없습니다!");
        }

        // TCPServer 자동 연결
        if (tcpServer == null)
        {
            tcpServer = FindObjectOfType<TCPServer>();
        }
    }

    void Update()
    {
        HandleLocalInput();
        HandleExternalInput();
    }

    void FixedUpdate()
    {
        MoveRobot();
    }

    /// <summary>
    /// 로컬 키보드 입력 처리 (Unity 안에서 직접 조작)
    /// </summary>
    void HandleLocalInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopRobot();
        }
    }

    /// <summary>
    /// 외부 Python 프로그램의 입력 처리
    /// </summary>
    void HandleExternalInput()
    {
        if (tcpServer == null) return;

        string command = tcpServer.lastCommand;
        if (string.IsNullOrEmpty(command)) return;

        // 명령 처리
        switch (command.ToUpper().Trim())
        {
            case "W":
            case "FORWARD":
                externalMoveZ = 1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "S":
            case "BACKWARD":
                externalMoveZ = -1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "A":
            case "LEFT":
                externalMoveZ = 0f;
                externalMoveX = -1f;
                externalStop = false;
                break;
            case "D":
            case "RIGHT":
                externalMoveZ = 0f;
                externalMoveX = 1f;
                externalStop = false;
                break;
            case "X":
                externalMoveZ = -1f;
                externalMoveX = 0f;
                externalStop = false;
                break;
            case "SPACE":
            case "STOP":
                externalStop = true;
                break;
            case "RELEASE":
                // 모든 입력 초기화
                externalMoveZ = 0f;
                externalMoveX = 0f;
                externalStop = true;
                break;
        }

        // 명령 처리 후 큐 초기화
        tcpServer.lastCommand = "";
    }

    /// <summary>
    /// 로봇을 이동시킵니다.
    /// </summary>
    void MoveRobot()
    {
        if (rb == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        // 로컬 입력 (키보드)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveZ = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveZ = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;
        if (Input.GetKey(KeyCode.X))
            moveZ = -1f;

        // 외부 입력이 있으면 로컬 입력을 무시
        if (externalMoveZ != 0 || externalMoveX != 0)
        {
            moveX = externalMoveX;
            moveZ = externalMoveZ;
        }

        // 정지 명령
        if (externalStop)
        {
            StopRobot();
            externalStop = false;
            return;
        }

        // 이동 적용
        Vector3 moveDirection = transform.forward * moveZ;
        moveDirection = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // 회전 적용
        float rotation = moveX * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void StopRobot()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        externalMoveZ = 0f;
        externalMoveX = 0f;

        Debug.Log("로봇이 멈췄습니다!");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
```

#### 4) Unity에서 설정하기

1. Unity로 돌아옵니다.
2. Hierarchy에서 **Robot** 오브젝트를 선택합니다.
3. Inspector에서 **RobotController** 컴포넌트를 찾습니다.
4. **Tcp Server** 필드에 TCPServer가 있는 오브젝트를 드래그합니다.
   - TCPServer를 같은 Robot 오브젝트에 붙였다면, Robot을 드래그하면 됩니다.

**TCPServer 오브젝트 추가:**
1. Hierarchy에서 빈 공간 우클릭 > **Create Empty** > 이름을 **"NetworkManager"**로 변경
2. NetworkManager를 선택하고 **Add Component > TCPServer** 추가
3. Inspector에서 **Port** 값을 `5000`으로 설정

최종 구조:
```
RobotScene
├── ...
├── NetworkManager    <- TCPServer (포트: 5000)
└── Robot             <- RobotController + Rigidbody
    ├── Body
    ├── Wheel_Right
    └── Wheel_Left
```

### 13-4. Python 측: TCP 클라이언트 프로그램 만들기

#### 1) Python 설치 확인

Python이 설치되어 있는지 확인합니다:

```bash
python --version
# 또는
python3 --version
```

Python이 설치되어 있지 않다면 https://www.python.org 에서 다운로드하여 설치합니다.

#### 2) 필요한 라이브러리 설치

터미널(명령 프롬프트)에서 다음 명령을 실행합니다:

```bash
pip install keyboard
```

> ⚠️ **주의**: `keyboard` 라이브러리는 관리자 권한이 필요할 수 있습니다.  
> Windows에서는 명령 프롬프트를 **관리자 권한으로 실행**해야 할 수 있습니다.  
> 또는 `pynput` 라이브러리를 대신 사용할 수도 있습니다:
> ```bash
> pip install pynput
> ```

#### 3) 기본 TCP 클라이언트 스크립트

텍스트 편집기(VS Code, 메모장 등)에서 새 파일을 만들고 **`robot_control.py`**로 저장합니다:

```python
import socket
import time
import sys

# ===== 설정 =====
HOST = "127.0.0.1"  # Unity가 실행 중인 컴퓨터 (로컬)
PORT = 5000          # Unity TCPServer의 포트 번호와 일치해야 함

def connect_to_unity():
    """Unity 서버에 연결합니다."""
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    try:
        client.connect((HOST, PORT))
        print(f"[연결 성공] Unity 서버에 연결되었습니다! ({HOST}:{PORT})")
        return client
    except ConnectionRefusedError:
        print("[연결 실패] Unity 서버가 실행 중인지 확인하세요!")
        print("  -> Unity에서 Play 버튼을 누르세요.")
        return None
    except Exception as e:
        print(f"[연결 실패] 오류: {e}")
        return None

def send_command(client, command):
    """Unity에 명령을 전송합니다."""
    try:
        message = command.encode("utf-8")
        client.sendall(message)
        print(f"  -> 전송: {command}")
    except BrokenPipeError:
        print("[오류] 연결이 끊어졌습니다!")
        return False
    except Exception as e:
        print(f"[전송 오류] {e}")
        return False
    return True

def print_controls():
    """조작법 안내를 출력합니다."""
    print("=" * 50)
    print("       로봇 원격 제어 프로그램")
    print("=" * 50)
    print()
    print("  조작법:")
    print("    W 또는 ↑  : 전진")
    print("    S 또는 ↓  : 후진")
    print("    A 또는 ←  : 좌회전")
    print("    D 또는 →  : 우회전")
    print("    X         : 후진")
    print("    SPACE     : 정지")
    print("    Q         : 프로그램 종료")
    print()
    print("=" * 50)

def main():
    print_controls()
    
    # Unity 연결
    print("\n[연결 중] Unity 서버에 연결을 시도합니다...")
    client = connect_to_unity()
    
    if client is None:
        print("\n프로그램을 종료합니다.")
        sys.exit(1)
    
    print("\n[준비 완료] 키보드를 눌러 로봇을 제어하세요!\n")
    
    try:
        # 간단한 키 입력 방식 (keyboard 라이브러리 사용)
        import keyboard
        
        last_key = None
        
        def on_key_event(event):
            nonlocal last_key
            if event.event_type == keyboard.KEY_DOWN:
                last_key = event.name.upper()
        
        # 키 이벤트 등록
        keyboard.hook(on_key_event)
        
        while True:
            if last_key is not None:
                key = last_key
                last_key = None
                
                # 명령 매핑
                command_map = {
                    "W": "W\n",
                    "UP": "W\n",
                    "S": "S\n",
                    "DOWN": "S\n",
                    "A": "A\n",
                    "LEFT": "A\n",
                    "D": "D\n",
                    "RIGHT": "D\n",
                    "X": "X\n",
                    "SPACE": "SPACE\n",
                    "Q": None,
                }
                
                if key == "Q":
                    print("\n[종료] 프로그램을 종료합니다.")
                    # 정지 명령 전송
                    send_command(client, "STOP\n")
                    break
                
                if key in command_map and command_map[key] is not None:
                    send_command(client, command_map[key])
            
            time.sleep(0.01)  # CPU 사용량 줄이기
    
    except ImportError:
        print("[대체 모드] keyboard 라이브러리 없이 실행합니다.")
        print("  명령을 직접 입력하세요 (W/S/A/D/X/SPACE/Q)\n")
        
        while True:
            command = input("명령 입력> ").strip().upper()
            
            if command == "Q":
                print("[종료] 프로그램을 종료합니다.")
                send_command(client, "STOP\n")
                break
            
            if command in ["W", "S", "A", "D", "X", "SPACE", "STOP", "RELEASE"]:
                send_command(client, command + "\n")
            else:
                print("  알 수 없는 명령입니다. W/S/A/D/X/SPACE/Q를 입력하세요.")
    
    finally:
        keyboard.unhook_all()
        client.close()
        print("[연결 해제] Unity 연결이 해제되었습니다.")

if __name__ == "__main__":
    main()
```

### 13-5. 테스트 방법

#### 1단계: Unity 실행

1. Unity 에디터에서 **▶ (Play)** 버튼을 누릅니다.
2. Console 창에 `[TCPServer] 서버 시작됨 - 포트: 5000` 메시지가 나타나는지 확인합니다.
3. `[TCPServer] Python에서 연결 대기 중...` 메시지가 보이면 정상입니다.

#### 2단계: Python 스크립트 실행

별도의 털미널(명령 프롬프트) 창을 열고:

```bash
python robot_control.py
```

또는 Python이 `python3`으로 설치된 경우:

```bash
python3 robot_control.py
```

#### 3단계: 연결 확인

Python 터미널에 다음 메시지가 나타나면 성공입니다:

```
[연결 성공] Unity 서버에 연결되었습니다! (127.0.0.1:5000)
```

Unity Console에는:

```
[TCPServer] 클라이언트 연결됨! (현재 1개)
```

#### 4단계: 로봇 제어

Python 터미널에서 키보드를 누르면 Unity의 로봇이 움직입니다!

| Python에서 누른 키 | Unity에서의 동작 |
|-------------------|-----------------|
| **W** 또는 **↑** | 로봇 전진 |
| **S** 또는 **↓** | 로봇 후진 |
| **A** 또는 **←** | 로봇 좌회전 |
| **D** 또는 **→** | 로봇 우회전 |
| **X** | 로봇 후진 |
| **SPACE** | 로봇 정지 |
| **Q** | 프로그램 종료 |

### 13-6. 고급: 키보드 없이 텍스트 입력으로 제어하기

`keyboard` 라이브러리 없이도 Python 프로그램은 동작합니다. 이 경우 터미널에서 명령어를 직접 입력합니다:

```
명령 입력> W
명령 입력> W
명령 입력> D
명령 输入> SPACE
명령 입력> Q
```

### 13-7. 문제 해결

#### 문제 1: "연결 실패" 메시지가 나타남

| 확인 사항 | 해결 방법 |
|----------|----------|
| Unity가 Play 모드인지 확인 | Unity에서 ▶ 버튼을 눌러 Play 모드로 전환 |
| 포트 번호 일치 확인 | Unity TCPServer의 port와 Python의 PORT 변수가 같은지 |
| 방화벽 차단 | Windows 방화벽에서 Python/Uner Unity를 허용 |
| localhost 사용 | HOST를 `127.0.0.1` 또는 `localhost`로 설정 |

#### 문제 2: 연결은 되었는데 로봇이 움직이지 않음

| 확인 사항 | 해결 방법 |
|----------|----------|
| RobotController의 Tcp Server 연결 | Inspector에서 TCPServer가 연결되어 있는지 확인 |
| 명령 수신 확인 | Unity Console에서 "수신 명령: W" 같은 메시지가 보이는지 확인 |
| Rigidbody 존재 | Robot 오브젝트에 Rigidbody가 있는지 확인 |

#### 문제 3: Python에서 "keyboard" 라이브러리 오류

```bash
# 관리자 권한으로 실행
# Windows: 명령 프롬프트를 관리자 권한으로 실행
# 또는 대체 라이브러리 사용:
pip install pynput
```

`pynput`을 사용하는 대체 코드:

```python
from pynput import keyboard

def on_press(key):
    try:
        if key == keyboard.Key.up:
            send_command(client, "W\n")
        elif key == keyboard.Key.down:
            send_command(client, "S\n")
        elif key == keyboard.Key.left:
            send_command(client, "A\n")
        elif key == keyboard.Key.right:
            send_command(client, "D\n")
        elif key == keyboard.Key.space:
            send_command(client, "SPACE\n")
        elif key == keyboard.Key.esc:
            return False  # 리스너 중지
    except AttributeError:
        # 일반 키 처리
        char = key.char
        if char and char.upper() in ['W', 'A', 'S', 'D', 'X']:
            send_command(client, char.upper() + "\n")
        elif char == 'q':
            return False

with keyboard.Listener(on_press=on_press) as listener:
    listener.join()
```

#### 문제 4: 여러 Python 클라이언트 동시 연결

현재 구현에서는 여러 Python 프로그램이 동시에 연결할 수 있습니다. 각 클라이언트는 별도 스레드에서 처리됩니다. 하지만 명령은 마지막에 수신된 것만 적용됩니다.

### 13-8. 전체 연결 구조도

```
┌───────────────────────────────────────────────────────┐
│                      Unity                            │
│                                                       │
│  ┌─────────────────┐      ┌────────────────────────┐  │
│  │   TCPServer     │      │    RobotController     │  │
│  │                 │      │                        │  │
│  │  포트: 5000     │─────>│  lastCommand 수신       │  │
│  │  수신 스레드     │      │  이동/회전 실행         │  │
│  │                 │      │                        │  │
│  └─────────────────┘      └────────────────────────┘  │
│           ▲                          │                 │
│           │ TCP/IP                   ▼                 │
│           │ (localhost:5000)  ┌──────────────┐         │
│           │                  │    Robot     │         │
│           │                  │  (Rigidbody) │         │
│           │                  └──────────────┘         │
└───────────┼───────────────────────────────────────────┘
            │
            │ TCP/IP
            │
┌───────────┼───────────────────────────────────────────┐
│           ▼          Python 프로그램                    │
│  ┌─────────────────┐                                  │
│  │ robot_control.py│                                  │
│  │                 │                                  │
│  │  키보드 입력     │                                  │
│  │    W, A, S, D   │                                  │
│  │    ↑, ↓, ←, →   │                                  │
│  │    SPACE, Q     │                                  │
│  │                 │                                  │
│  │  TCP 클라이언트   │                                  │
│  │  127.0.0.1:5000 │                                  │
│  └─────────────────┘                                  │
└───────────────────────────────────────────────────────┘
```

### 13-9. 명령 프로토콜 정리

Unity와 Python 사이에서 주고받는 명령어 목록:

| 명령어 | 의미 | Unity에서의 동작 |
|--------|------|-----------------|
| `W` | 전진 | 로봇이 앞쪽으로 이동 |
| `S` | 후진 | 로봇이 뒤쪽으로 이동 |
| `A` | 좌회전 | 로봇이 왼쪽으로 회전 |
| `D` | 우회전 | 로봇이 오른쪽으로 회전 |
| `X` | 후진 | S와 동일 |
| `SPACE` | 정지 | 로봇의 속도를 0으로 |
| `STOP` | 정지 | SPACE와 동일 |
| `RELEASE` | 입력 해제 | 모든 이동 입력 초기화 |
| `FORWARD` | 전진 | W와 동일 (영문 확장) |
| `BACKWARD` | 후진 | S와 동일 (영문 확장) |
| `LEFT` | 좌회전 | A와 동일 (영문 확장) |
| `RIGHT` | 우회전 | D와 동일 (영문 확장) |

> 💡 **팁**: 명령어는 대소문자를 구분하지 않습니다. `w`, `W`, `Woow` 모두 `W`로 처리됩니다.  
> 각 명령은 줄바꿈(`\n`)으로 구분하여 전송합니다.

### 13-10. 최종 프로젝트 구조

```
RobotTutorial/
├── Assets/
│   ├── Materials/
│   │   ├── BodyMaterial
│   │   ├── WheelMaterial
│   │   └── GroundMaterial
│   ├── PhysicMaterials/
│   │   └── RobotMaterial
│   ├── Scripts/
│   │   ├── RobotController.cs    <- 로봇 제어 (외부 입력 지원)
│   │   └── TCPServer.cs          <- TCP 서버
│   └── Scenes/
│       └── RobotScene.unity
│
└── Python/
    └── robot_control.py          <- Python 클라이언트 프로그램
```

---

> **출처**: NVIDIA Omniverse Isaac Sim 튜토리얼 2.2 "Add Simple Objects"를 Unity에 맞게 번안 및 확장  
> **추가 참고**: Unity- Python TCP/IP 통신
