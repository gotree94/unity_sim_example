# 1단계: Unreal Engine 5 설치 및 기초 설정

## 1-1. Unreal Engine 5 설치

### Epic Games Launcher 설치
1. https://www.unrealengine.com/download 에서 Epic Games Launcher 다운로드
2. 설치 후 로그인 (Epic Games 계정 필요)

### UE 5.8 설치
1. Epic Games Launcher → Unreal Engine 탭 → 라이브러리
2. "エンジンの追加" 클릭 → **5.8** 버전 선택
3. 설치 옵션:
   - **Core Components**: 필수
   - ** starter Content**: 선택 (기본 예제 포함)
   - **Templates and Feature Packs**: 선택
4. 설치 경로: `C:\Program Files\Epic Games\UE_5.8` (기본값 권장)
5. 설치 완료까지 대기 (약 30-60GB 필요)

### 검증
- Epic Games Launcher → 라이브러리 → UE 5.8 "실행" 클릭
- Unreal Editor가 정상적으로 열리면 성공

---

## 1-2. 프로젝트 생성

### 새 프로젝트 만들기
1. Unreal Editor 실행
2. "새 프로젝트" 선택
3. 템플릿: **Third Person** 또는 **Blank** 선택
4. 프로젝트 유형: **C++** 선택 (Python 통합에 유리)
5. 프로젝트 이름: `RobotSimulation` 또는 원하는 이름
6. 위치: `C:\Users\Administrator\Desktop\` 선택
7. "생성" 클릭

### 프로젝트 구조 확인
```
RobotSimulation/
├── Source/           # C++ 소스 코드
├── Content/          # 에셋 (메시, 머티리얼 등)
├── Config/           # 프로젝트 설정
├── Binaries/         # 빌드 결과물
└── RobotSimulation.uproject  # 프로젝트 파일
```

---

## 1-3. Python 통합 설정 (선택사항)

### 왜 Python인가?
- Unity 튜토리얼과 동일한 Python TCP/IP 통신 구현 가능
- 빠른 프로토타이핑 및 스크립팅에 유리
- UE5에서는 C++이 주 언어이지만, Python 플러그인으로 보완 가능

### Python 플러그인 설치
1. Unreal Editor → 편집 → 플러그인
2. 검색: "Python"
3. **Python Editor Script Plugin** 활성화
4. **Editor Scripting Utilities** 활성화
5. Unreal Editor 재시작

### Python 경로 설정
1. 편집 → 프로젝트 설정 → 엔진 → Python
2. Python Interpreter 경로 설정:
   - Windows: `C:\Python39\python.exe` (설치된 Python 경로)
   - 또는 Anaconda 환경 사용 시 해당 경로

### 검증
1. Unreal Editor → 편집 → 개발자 도구 → Output Log
2. Python 콘솔에서 테스트:
   ```python
   import unreal
   unreal.log("Python 연결 성공!")
   ```

---

## 1-4. C++ 개발 환경 설정

### Visual Studio 설치 (Windows)
1. Visual Studio 2022 Community 또는 Professional 설치
2. 워크로드 선택:
   - **Game development with C++**
   - **Desktop development with C++**
3. 설치 완료

### 프로젝트 C++ 설정
1. UE 프로젝트에서 편집 → 프로젝트 구조에서 자동으로 .sln 파일 생성
2. 또는 수동으로: 우클릭 → Unreal Engine → Generate Visual Studio project files

### C++ 클래스 생성 테스트
1. Content Browser → 우클릭 → New C++ Class
2. 부모 클래스: **Actor** 선택
3. 클래스 이름: `TestActor`
4. "클래스 생성" 클릭
5. Visual Studio에서 `TestActor.h`, `TestActor.cpp` 파일 확인

---

## 1-5. Unity와의 주요 차이점

| 항목 | Unity | Unreal Engine 5 |
|------|-------|-----------------|
| 주 언어 | C# | C++ (Blueprint 지원) |
| 에디터 | Unity Editor | Unreal Editor |
| 프로젝트 구조 | Assets/ | Content/ + Source/ |
| 빌드 시스템 | Unity Build | Unreal Build Tool (UBT) |
| 플러그인 형식 | Unity Package | .uplugin 파일 |
| Python 지원 | 기본 미지원 | Python Editor Script Plugin |
| 소스 코드 편집 | Visual Studio/Rider | Visual Studio/Rider |

### 다음 단계
- [2단계: C++ 기반 로봇 시뮬레이션](./02_UE_CPP_Robot_Tutorial.md)
- [3단계: Blueprint 기반 로봇 시뮬레이션](./03_UE_Blueprint_Robot_Tutorial.md)