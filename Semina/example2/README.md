# Ubisam-Semina

## 개요
이 프로젝트는 Spring Boot 기반의 예제 프로젝트로, REST API와 JPA를 활용한 데이터 관리 기능을 포함하고 있습니다.

## 폴더 구조
- `src/main/java/com/ubisam/example2/helloes/` : Hello 관련 엔티티, 핸들러, 리포지토리
- `src/main/java/com/ubisam/example2/Worlds/` : World 관련 엔티티, 리포지토리
- `src/main/resources/` : 애플리케이션 설정 파일
- `src/test/java/com/ubisam/example2/` : 테스트 코드

## 주요 기능
- Hello 및 World 엔티티 관리
- REST API를 통한 CRUD
- JPA를 이용한 데이터베이스 연동
- MockMvc를 활용한 웹 테스트

## 실행 방법
1. `mvnw` 또는 `mvnw.cmd`로 빌드 및 실행
2. `application.properties`에서 DB 설정 확인
3. API 테스트는 테스트 코드(`Example2ApplicationTests.java`) 참고

## 개발 환경
- Java 17 이상
- Spring Boot
- Maven

## 문의
궁금한 점이나 개선 사항은 이슈 또는 PR로 남겨주세요.
