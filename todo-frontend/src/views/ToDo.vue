<template>
  <div class="todo-page-container">
    <div class="daily-panel">
      <div class="projects-header">
        <h2>Don'ts</h2>
        <div class="add-todo-button" @click="isChallengePopupVisible = true">
          <span>+</span>
        </div>
      </div>
      <div class="category-buttons">
        <button
          @click="selectCategory('Günlük')"
          :class="{ 'selected': activeCategory === 'Günlük' }"
          class="tema-butonu category-button"
        >
          Günlük
        </button>
        <button
          @click="selectCategory('Haftalık')"
          :class="{ 'selected': activeCategory === 'Haftalık' }"
          class="tema-butonu category-button"
        >
          Haftalık
        </button>
        <button
          @click="selectCategory('Aylık')"
          :class="{ 'selected': activeCategory === 'Aylık' }"
          class="tema-butonu category-button"
        >
          Aylık
        </button>
      </div>
    </div>

    <div class="todos-panel">
      <div v-if="activeCategory">
        <h2>{{ activeCategory }}</h2>
        <ul class="todo-list">
          <li v-for="task in tasks" :key="task.id" :class="{ 'completed': task.isCompleted }">
            <input type="checkbox" v-model="task.isCompleted" @change="updateTask(task)" />
            <span>{{ task.title }}</span>
            <button @click="deleteTask(task.id)" class="delete-button">×</button>
          </li>
        </ul>
        <div v-if="tasks.length === 0" class="no-project-selected">
          <p>Kurtulmak istediğiniz bir {{activeCategory}} alışkanlık ekleyin.</p>
        </div>
      </div>
    </div>

    <div v-if="isChallengePopupVisible" class="popup-overlay" @click.self="isChallengePopupVisible = false">
      <div class="popup-box tema-kutusu">
        <h2>Ne Yapmaman Gerek</h2>
        <form @submit.prevent="createTaskFromPopup">
          <input v-model="newChallengeName" placeholder="..." />
          <div class="popup-category-selector">
            <button
              type="button"
              @click="selectPopupCategory('Günlük')"
              :class="{ 'selected': newTaskCategory === 'Günlük' }"
              >Günlük
            </button>
            <button
              type="button"
              @click="selectPopupCategory('Haftalık')"
              :class="{ 'selected': newTaskCategory === 'Haftalık' }"
              >Haftalık
            </button>
            <button
              type="button"
              @click="selectPopupCategory('Aylık')"
              :class="{ 'selected': newTaskCategory === 'Aylık' }"
              >Aylık
            </button>
          </div>
          <button type="submit" class="tema-butonu">Oluştur</button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';

// --- YENİ EKLENEN/GÜNCELLENEN KISIM ---
// Proje yönetimiyle ilgili tüm eski değişkenler kaldırıldı.
// Artık sadece görevleri (tasks) ve aktif kategoriyi takip ediyoruz.
const tasks = ref([]);
const activeCategory = ref('Günlük');
const newTaskCategory = ref('Günlük'); 
const newTaskTitle = ref('');
const newChallengeName = ref('');
const userId = localStorage.getItem('userId');
const isChallengePopupVisible = ref(false);

// Seçilen kategoriye göre görevleri backend'den getirir.
async function fetchTasks() {
  if (!userId) return;
  const res = await fetch(`http://localhost:5000/api/users/${userId}/todos?category=${activeCategory.value}`);
  if (res.ok) {
    tasks.value = await res.json();
  }
}

function selectCategory(category) {
  activeCategory.value = category;
}
function selectPopupCategory(category) {
  newTaskCategory.value = category;
}

watch(activeCategory, fetchTasks);

async function createTaskFromPopup() {
    if (!newChallengeName.value.trim() || !userId) return;
    
    await createTaskApi(newChallengeName.value, newTaskCategory.value);
    
    isChallengePopupVisible.value = false;
    newChallengeName.value = '';
}

async function createTaskApi(title, category) {
    const res = await fetch(`http://localhost:5000/api/users/${userId}/todos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            title: title,
            category: category,
            isCompleted: false
        })
    });
    if (res.ok) {
        // Eğer eklenen görev şu an görüntülenen kategorideyse, listeyi yenile
        if (category === activeCategory.value) {
            fetchTasks();
        }
    } else {
        alert('Görev oluşturulamadı.');
    }
}

// Bir görevin "tamamlandı" durumunu günceller
async function updateTask(task) {
  const res = await fetch(`http://localhost:5000/api/todos/${Id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(task)
  });
}

async function deleteTask(taskId) {
  // Onay yerine şifre sor
  const password = prompt('Görevi silmek için lütfen şifrenizi girin:');

  // Eğer kullanıcı iptal ederse veya şifre girmezse işlemi durdur
  if (password === null || password === '') {
    return;
  }

  const res = await fetch(`http://localhost:5000/api/todos/${taskId}`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    // Gövdeye şifreyi ve kullanıcı ID'sini ekle
    body: JSON.stringify({ 
      password: password,
      userId: parseInt(userId) // userId'yi sayıya çevirerek gönder
    })
  });

  if (res.ok) {
    fetchTasks(); // Görev listesini yenile
  } else if (res.status === 401) {
    alert('Geçersiz şifre! Görev silinemedi.');
  } else {
    alert('Görev silinirken bir hata oluştu.');
  }
}

onMounted(fetchTasks);
</script>

<style scoped>
.todo-page-container {
  display: flex;
  width: 100vw;
  height: 100vh;
  background-color: var(--kutu-arkaplan);
  color: var(--yazi-rengi);
}

.daily-panel {
  width: 350px;
  background-color: var(--arka-plan);
  color: var(--yazi-rengi);
  padding: 2em;
  border-right: 1px solid #e0e0e0;
  display: flex;
  flex-direction: column;
}

.projects-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2em;
}

.add-todo-button {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: var(--kutu-arkaplan);
  cursor: pointer;
  transition: transform 0.2s ease;
  position: relative;
}

.add-todo-button span {
  color: var(--buton-yazi);
  font-size: 32px;
  font-weight: lighter;
  position: absolute;
  top: 42%;
  left: 50%;
  transform: translate(-50%, -50%);
}
.add-todo-button:hover {
  transform: scale(1.1);
  background-color: rgba(254, 227, 19, 1);
}

.todos-panel {
  flex-grow: 1;
  padding: 2em 4em;
  overflow-y: auto;
}

.todo-form {
  display: flex;
  margin-bottom: 2em;
}
.todo-form input {
  flex-grow: 1;
  margin-right: 10px;
  padding: 0.8em;
  border: none;
  border-radius: 8px;
}
.todo-form button {
  width: 100px;
  flex-shrink: 0;
}

.todo-list {
  list-style: none;
  padding: 0;
}
.todo-list li {
  display: flex;
  align-items: center;
  padding: 1em;
  border-bottom: 1px solid #eee;
}
.todo-list li.completed span {
  text-decoration: line-through;
  opacity: 0.6;
}
.todo-list li input[type="checkbox"] {
  margin-right: 1em;
  width: 20px;
  height: 20px;
}
.todo-list li span {
  flex-grow: 1;
}

.delete-button {
  background: none;
  border: none;
  color: #e74c3c;
  font-size: 20px;
  cursor: pointer;
  visibility: hidden;
  opacity: 0;
  transition: opacity 0.2s ease;
}
.todo-list li:hover .delete-button {
  visibility: visible;
  opacity: 1;
}

.no-project-selected {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  font-size: 1.5em;
  color: #ffffffff;
  border:none;
}

.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
}

.popup-box {
  padding: 2em;
  background-color: var(--kutu-arkaplan);
  border-radius: 12px;
  width: 100%;
  max-width: 400px;
  z-index: 1001;
}

.popup-box input {
  margin-bottom: 1em;
}

.category-buttons {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.category-button {
  width: 100%;
  text-align: left;
  padding: 1em;
  font-size: 1em;
  background-color: var(--input-arkaplan);
  color: var(--yazi-rengi);
  border: none;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.category-button:hover {
  background-color: var(--buton-arkaplan);
  color: var(--buton-yazi);
}

.category-button.selected {
  background-color: var(--buton-arkaplan);
  color: var(--buton-yazi);
  font-weight: bold;
}

.popup-category-selector {
  display: flex;
  justify-content: space-between;
  margin-top: 1em;
  margin-bottom: 1.5em;
  gap: 10px;
}

.popup-category-selector button {
  flex-grow: 1;
  padding: 0.8em;
  border: 1px solid #ccc;
  background-color: var(--buton-arkaplan);
  color: var(--buton-yazi);
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.popup-category-selector button.selected {
  background-color: var(--selected-buton-arkaplan);
  color: var(--selected-buton-yazi);
  border:none;
}

</style>
