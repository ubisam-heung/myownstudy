<script setup lang="ts">
import { ref, onMounted } from 'vue'

// 폼 입력값
const name = ref('')
const email = ref('')

// 서버에서 받은 목록
const hellos = ref<any[]>([])

// POST: 데이터 저장
const submitForm = async () => {
  if (!name.value || !email.value) {
    alert('이름과 이메일을 입력하세요')
    return
  }

  try {
    const res = await fetch('http://localhost:8080/api/hello', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: name.value,
        email: email.value,
      }),
    })

    if (!res.ok) {
      throw new Error('저장 실패')
    }

    const saved = await res.json()

    // 목록에 바로 추가
    hellos.value.push(saved)

    // 입력값 초기화
    name.value = ''
    email.value = ''
  } catch (e) {
    console.error(e)
    alert('서버 통신 오류')
  }
}

// GET: 목록 조회
const loadList = async () => {
  const res = await fetch('http://localhost:8080/api/hello')
  hellos.value = await res.json()
}

// 컴포넌트 로드 시 목록 조회
onMounted(loadList)
</script>

<template>
  <div class="container">
    <h1>Spring Boot + Vue 3 연동 테스트</h1>

    <!-- 입력 폼 -->
    <form @submit.prevent="submitForm" class="form">
      <input
        type="text"
        v-model="name"
        placeholder="이름"
      />
      <input
        type="email"
        v-model="email"
        placeholder="이메일"
      />
      <button type="submit">저장</button>
    </form>

    <!-- 목록 -->
    <h2>저장된 목록</h2>
    <ul>
      <li v-for="h in hellos" :key="h.id">
        {{ h.name }} ({{ h.email }})
      </li>
    </ul>
  </div>
</template>

<style scoped>
.container {
  max-width: 500px;
  margin: 40px auto;
}

h1, h2 {
  text-align: center;
}

.form {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
}

input {
  flex: 1;
  padding: 8px;
}

button {
  padding: 8px 16px;
  background: #42b883;
  border: none;
  color: white;
  cursor: pointer;
}

button:hover {
  background: #369f6b;
}

ul {
  list-style: none;
  padding: 0;
}

li {
  padding: 6px 0;
  border-bottom: 1px solid #ddd;
}
</style>
