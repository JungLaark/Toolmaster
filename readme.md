
# 수정 및 추가해야 할 기능들 
완료 표시 필요
***
## 사용자 관리 메뉴 신설(권한 관리)
1. 로그인 화면
2. 로그인 
- 비밀번호 암/복호화
3. 로그아웃 
4. 세션 관리 

로그인하기 전 까지는 메뉴 보이지 않게 하기. 
admin, develop 이렇게 하면 안될거같은데??
새로운 계정을 만들어야 함

연구소 계정 -> 다 할 수 있도록 할 것 
ateciot-lab
lab@12345
서비스팀 계정 -> 로그 확인만 할 수 있도록 할 것 
ateciot-service
service@12345


***
## 유효성 검사 
1. 주소 확인 
2. 로그 메뉴 
- 접근 가능한 hw 인지 아닌지 체크할 필요있음
***
## 로그 메뉴 
1. 로그 조회 후 단어 검색 할 수 있도록 해야 함
***
## UI
1. splash image
2. 조회 시 로딩 화면
***
## 기타 
1. ESN 이 아닌 EMS 가 구축되어져 있는 상황이라면 매장리스트 항목이 필요있을까?
- 이건 그냥 삭제해주면 될거같음.


[192.168.30.30]
emart daemon : 8081
emart ESN web : 8280

homeplus daemon : 8181
homeplus ESN web : 8180

[61.33.142.222]
NH daemon : 8181
NH ESN web : 8080


# 2023-10-30 
1. 1차 개발 완료 보고 개선 사항 
   1. 비번 암호화 
	-> 캡쳐 다시하기 							완료 
   2. DB, SSH 설정 화면 필요하지 않다. 
	-> ESN 설정만 있으면 된다. 
	-> Gen Mini ssh 비번이 달라 분기할 필요성이 있다. 
   3. delete 기능만 있어도 될거같다. 
	-> 하나의 매장, 하나의 테이블 선택하면 다 조회되게끔 delete 하게끔 
	-> 하나의 매장, 다수의 테이블 선택하면 조회가 안되고 truncate 하게끔 
     단축키 말고 버튼 or 체크 박스 등 만들어 놓기. 
   4. upload 기능 만들기 
	-> 괜찮은 기능인거 같음 
   5. 로그인 화면 없에고 data 삭제 시 계정 정보 물어보기
	-> 이 기능은 관리자 기능이다 카믄서 
   6. tail 기능 
	-> 로그 선택해서 클릭하면 cmd 창 열려서 자동으로 tail-f 되게끔 
	-> 이건 재미날꺼같다. 
   7. 사용자 메뉴얼 만들고 각 유지보수 사이트에 설치할 수 있도록 하기. 
   8. EMS Log 다운로드 메뉴
	-> 해당 매장이 Gen2+ 인지 GenServer 인지 알 수 있는 방법

![image](https://github.com/user-attachments/assets/f9afbc1a-a11d-4c02-ab1f-5173b7959e7a)

![image](https://github.com/user-attachments/assets/08f49cfd-f40a-4b9c-855d-feb3557f0ddb)





