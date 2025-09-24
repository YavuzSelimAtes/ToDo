<template>
<div>
<div>
  <Leaderboard v-if="isLeaderboardVisible" :user-id="userId" @close="isLeaderboardVisible = false" />
  <transition name="slide-fade">
    </transition>
  </div>
  <transition name="slide-fade">
      <div v-if="isSidebarOpen" class="sidebar">
        <div class="user-profile">
          <h3 v-if="user" class="username">{{ user.username }}</h3>
<div class="stat-item-wrapper">
  <button 
    class="stat-button" 
    @mouseenter="handleMouseEnter('Günlük')" 
    @mouseleave="handleMouseLeave()">
    <span>Günlük: {{ user.dailyTasks }}</span>
  </button>
  
  <transition name="details-slide">
    <div v-if="visibleDetails === 'Günlük'" class="details-panel">
      <div class="stat-line"><span>Oluşturulan:</span> <span>{{ user.totalDailyTasksCreated }}</span></div>
      <div class="stat-line"><span>Başarılı:</span> <span class="text-success">{{ user.dailyTasksCompleted }}</span></div>
      <div class="stat-line"><span>Başarısız:</span> <span class="text-danger">{{ user.dailyTasksFailed }}</span></div>
      <div class="stat-line"><span>Kazanılan Puan:</span> <span>{{ totalDailyScore }}</span></div>
    </div>
  </transition>
</div>

<div class="stat-item-wrapper">
  <button 
    class="stat-button" 
    @mouseenter="handleMouseEnter('Haftalık')" 
    @mouseleave="handleMouseLeave()">
    <span>Haftalık: {{ user.weeklyTasks }}</span>
  </button>

  <transition name="details-slide">
    <div v-if="visibleDetails === 'Haftalık'" class="details-panel">
      <div class="stat-line"><span>Oluşturulan:</span> <span>{{ user.totalWeeklyTasksCreated }}</span></div>
      <div class="stat-line"><span>Başarılı:</span> <span class="text-success">{{ user.weeklyTasksCompleted }}</span></div>
      <div class="stat-line"><span>Başarısız:</span> <span class="text-danger">{{ user.weeklyTasksFailed }}</span></div>
      <div class="stat-line"><span>Kazanılan Puan:</span> <span>{{ totalWeeklyScore }}</span></div>
    </div>
  </transition>
</div>

<div class="stat-item-wrapper">
  <button 
    class="stat-button" 
    @mouseenter="handleMouseEnter('Aylık')" 
    @mouseleave="handleMouseLeave()">
    <span>Aylık: {{ user.monthlyTasks }}</span>
  </button>
  
  <transition name="details-slide">
    <div v-if="visibleDetails === 'Aylık'" class="details-panel">
      <div class="stat-line"><span>Oluşturulan:</span> <span>{{ user.totalMonthlyTasksCreated }}</span></div>
      <div class="stat-line"><span>Başarılı:</span> <span class="text-success">{{ user.monthlyTasksCompleted }}</span></div>
      <div class="stat-line"><span>Başarısız:</span> <span class="text-danger">{{ user.monthlyTasksFailed }}</span></div>
      <div class="stat-line"><span>Kazanılan Puan:</span> <span>{{ totalMonthlyScore }}</span></div>
    </div>
  </transition>
</div>
          <div class="score-container">
            <button class="score-button">Skor: {{user.score}}</button>
          </div>  
          <div class="sidebar-actions">
            <button @click="logout" class="logout-button">
              Çıkış Yap
            </button>
            <button @click="deleteUserAccount" class="delete-account-button">
              Hesabı Sil
            </button>
          </div>    
        </div>
      </div>
  </transition>
  <div v-if="isSidebarOpen" class="sidebar-overlay" @click="isSidebarOpen = false"></div>
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
      <div class="sidebar-toggle-container">
        <div class="bottom-left-actions">
          <button @click="isSidebarOpen = !isSidebarOpen" class="sidebar-toggle-button">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="3" y1="12" x2="21" y2="12"></line>
              <line x1="3" y1="6" x2="21" y2="6"></line>
              <line x1="3" y1="18" x2="21" y2="18"></line>
            </svg>
          </button>
          <button @click="isLeaderboardVisible = true" class="leaderboard-toggle-button">
            Leaderboard
          </button>
        </div>
      </div>
    </div>

    <div class="todos-panel">
      <div v-if="activeCategory">
        <h2>
          <span v-if="activeCategory === 'Günlük'" class="header-date">
            {{ selectedDate.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }) }}
          </span>

          <span v-if="activeCategory === 'Haftalık'" class="header-date">
            {{ selectedWeek }}. Hafta {{ selectedYear }}
          </span>

          <span v-if="activeCategory === 'Aylık'" class="header-date">
            {{ selectedMonthDate.toLocaleDateString('tr-TR', { month: 'long', year: 'numeric' }) }}
          </span>
        </h2>
        <button v-if="activeCategory === 'Günlük'" @click="isCalendarVisible = !isCalendarVisible" class="calendar-toggle-button" ref="calendarButtonRef">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zM9 14H7v-2h2v2zm4 0h-2v-2h2v2zm4 0h-2v-2h2v2zm-8 4H7v-2h2v2zm4 0h-2v-2h2v2zm4 0h-2v-2h2v2z"/>
          </svg>
        </button>

        <button v-if="activeCategory === 'Haftalık'" @click="isWeekSelectorVisible = !isWeekSelectorVisible" class="calendar-toggle-button" ref="weekSelectorButtonRef">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20 3h-1V1h-2v2H7V1H5v2H4c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 18H4V8h16v13z"/>
          </svg>
        </button>

        <button v-if="activeCategory === 'Aylık'" @click="isMonthSelectorVisible = !isMonthSelectorVisible" class="calendar-toggle-button" ref="monthSelectorButtonRef">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19 19H5V8h14m-3-7v2H8V1H6v2H5c-1.11 0-2 .89-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2h-1V1m-1 11h-5v5h5v-5Z"/>
          </svg>
        </button>
        <div v-if="activeCategory === 'Günlük' && isCalendarVisible" class="calendar-container" ref="calendarContainerRef">
          <VDatePicker v-model="selectedDate" :is-dark="isDarkMode" expanded/>
        </div>

        <div v-if="activeCategory === 'Haftalık' && isWeekSelectorVisible" class="week-selector-panel" ref="weekSelectorPanelRef">
          <div class="year-navigator">
            <button @click="prevYear">&lt;</button>
            <h3>{{ selectedYear }}</h3>
            <button @click="nextYear">&gt;</button>
          </div>
          <div class="week-grid">
            <div 
              v-for="week in 52" 
              :key="week" 
              class="week-item"
              :class="{ 'selected': week === selectedWeek, 'current': week === currentWeekNumber }"
              @click="selectWeek(week)">
              {{ week }}
            </div>
          </div>
        </div>

        <div v-if="activeCategory === 'Aylık' && isMonthSelectorVisible" class="month-selector-panel" ref="monthSelectorPanelRef">
          <div class="year-navigator">
            <button @click="prevYearForMonthView">&lt;</button>
              <h3>{{ selectedMonthDate.getFullYear() }}</h3>
            <button @click="nextYearForMonthView">&gt;</button>
          </div>
          <div class="month-grid">
            <div 
              v-for="(ay, index) in aylar" 
              :key="ay" 
              class="month-item"
              :class="{ 'selected': index === selectedMonthDate.getMonth(), 'current': index === currentMonthIndex && selectedMonthDate.getFullYear() === currentYear }"
              @click="selectMonth(index)">
              {{ ay }}
            </div>
          </div>
        </div>
        <ul class="todo-list">
  <li v-for="task in tasks" :key="task.id" 
    :class="{ 'completed': task.state === 2, 'disabled': task.state === 0, 'failed': task.state === 3 }">
    <input 
      type="checkbox" 
      :checked="task.state === 2"
      :disabled="task.state !== 1"
      @change="toggleTaskState(task)" 
    />
    <span>{{ task.title }}</span>
    <button @click="deleteTask(task.id)" class="delete-button">×</button>
  </li>
</ul>
        <div v-if="tasks.length === 0" class="no-project-selected">
          <p>Kurtulmak istediğiniz bir {{activeCategory}} alışkanlık ekleyin.</p>
        </div>
      </div>
    </div>

    <div v-if="isLoading" class="loading-spinner">
         <div></div><div></div><div></div><div></div>
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
</div>  
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
import Leaderboard from './Leaderboard.vue';
import { isDarkMode } from '../themeStore.js';
import 'v-calendar/style.css';
import { DatePicker as VDatePicker } from 'v-calendar';

// --- Değişken Tanımlamaları ---
const tasks = ref([]);
const user = ref(null);
const activeCategory = ref('Günlük');
const newTaskCategory = ref('Günlük');
const newChallengeName = ref('');
const userId = localStorage.getItem('userId');
const isChallengePopupVisible = ref(false);
const selectedDate = ref(new Date());
const isCalendarVisible = ref(false);
const calendarContainerRef = ref(null);
const calendarButtonRef = ref(null);
const selectedYear = ref(new Date().getFullYear());
const selectedWeek = ref(getCurrentWeekNumber());
const weekSelectorPanelRef = ref(null); // Hafta seçici paneline referans
const weekSelectorButtonRef = ref(null);
const isWeekSelectorVisible = ref(true);
const currentWeekNumber = ref(getCurrentWeekNumber());
const selectedMonthDate = ref(new Date());
const isMonthSelectorVisible = ref(true);
const monthSelectorButtonRef = ref(null);
const monthSelectorPanelRef = ref(null);
const aylar = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];
const currentMonthIndex = ref(new Date().getMonth());
const currentYear = ref(new Date().getFullYear());
const isLoading = ref(false);
const isSidebarOpen = ref(false);
const detailsTimer = ref(null);  
const visibleDetails = ref(null);
const isLeaderboardVisible = ref(false);

// --- API Fonksiyonları ---

// YÜKLENİYOR GÖSTERGESİ İÇİN GÜNCELLENDİ
async function fetchTasks() {
  if (!userId) return;
  isLoading.value = true; // Yükleme başladı
  try {
    let apiUrl = `/api/users/${userId}/todos?category=${activeCategory.value}`;

    if (activeCategory.value === 'Günlük') {
      const startOfDay = new Date(selectedDate.value);
      startOfDay.setHours(0, 0, 0, 0);
      const endOfDay = new Date(selectedDate.value);
      endOfDay.setHours(23, 59, 59, 999);
      apiUrl += `&startDate=${startOfDay.toISOString()}&endDate=${endOfDay.toISOString()}`;
    } 
    else if (activeCategory.value === 'Haftalık') {
      apiUrl += `&year=${selectedYear.value}&week=${selectedWeek.value}`;
    }
    else if (activeCategory.value === 'Aylık') {
      const year = selectedMonthDate.value.getFullYear();
      const month = selectedMonthDate.value.getMonth() + 1;
      apiUrl += `&year=${year}&month=${month}`;
    }

    const res = await fetch(apiUrl);
    if (res.ok) {
      tasks.value = await res.json();
    }
  } catch (error) {
    alert("Görevler yüklenirken bir hata oluştu.");
  } finally {
    isLoading.value = false; // Yükleme bitti (başarılı ya da hatalı)
  }
}

async function fetchUser() {
  if (!userId) return;
  try {
    const res = await fetch(`/api/users/${userId}`);
    if (res.ok) {
      user.value = await res.json();
    } else {
      console.error("Kullanıcı bilgisi alınamadı.");
      // İsteğe bağlı: Kullanıcı bulunamazsa çıkış yaptırılabilir.
    }
  } catch (error) {
    console.error("Kullanıcı bilgisi alınırken hata:", error);
  }
}

async function deleteUserAccount() {
  const promptMessage = 'Hesabınızı kalıcı olarak silmek istediğinizden emin misiniz? Bu işlem geri alınamaz ve tüm verileriniz kaybolacaktır.\n\nDevam etmek için lütfen şifrenizi girin:';
  const password = prompt(promptMessage);

  if (password === null || password === '') {
    return; // Kullanıcı iptal etti veya bir şey girmedi
  }

  try {
    const res = await fetch(`/api/users/${userId}`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password: password }) // DTO'ya uygun şekilde gönder
    });

    if (res.ok) {
      alert("Hesabınız başarıyla silindi.");
      // Kullanıcıyı çıkışa yönlendir
      localStorage.removeItem('userId');
      window.location.href = '/'; // Ana sayfaya veya giriş sayfasına yönlendir
    } else if (res.status === 401) {
      alert('Geçersiz şifre! Hesap silinemedi.');
    } else {
      alert('Hesap silinirken bir hata oluştu.');
    }
  } catch (error) {
    console.error("Hesap silinirken hata:", error);
    alert('Sunucuyla iletişim kurulamadı.');
  }
}

async function createTaskApi(title, category) {
    const taskData = {
        title: title,
        category: category,
        createdAt: new Date().toISOString()
    };
    const res = await fetch(`/api/users/${userId}/todos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(taskData)
    });

    if (res.ok) {
        if (category === activeCategory.value) {
            fetchTasks();
        }
        return await res.json(); 
    } else {
        alert('Görev oluşturulamadı');
        return false;
    }
}

async function createTaskFromPopup() {
    if (!newChallengeName.value.trim() || !userId) return;
    
    const updatedUser = await createTaskApi(newChallengeName.value, newTaskCategory.value);
    
    if (updatedUser) {
        isChallengePopupVisible.value = false;
        newChallengeName.value = '';
        user.value = updatedUser;
        if (newTaskCategory.value !== activeCategory.value) {
            selectCategory(newTaskCategory.value);
        }else {
            selectCategory(newTaskCategory.value);
        }
    }
}

async function deleteTask(taskId) {
  const promptMessage = 'Bu alışkanlık şablonunu ve sadece bu görevi kalıcı olarak silmek istediğinizden emin misiniz?\n\nBu işlem sonrası bu alışkanlık için gelecekte yeni görevler oluşturulmayacaktır.\n\nLütfen şifrenizi girin:';
  const password = prompt(promptMessage);
  
  if (password === null || password === '') return;

  const res = await fetch(`/api/todos/template/${taskId}`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ 
      password: password,
      userId: parseInt(userId)
    })
  });

  if (res.ok) {
    alert("Alışkanlık başarıyla silindi.");
    fetchTasks();
    const updatedUser = await res.json();
    user.value = updatedUser;
  } else if (res.status === 401) {
    alert('Geçersiz şifre! Görev silinemedi.');
  } else {
    alert('Görev silinemedi.');
  }
}

// --- DİĞER FONKSİYONLAR ---

// Bu fonksiyon, checkbox'a tıklandığında çalışır.
async function toggleTaskState(task) {
  // Sadece durumu "Açık" (1) ise işlem yap
  if (task.state === 1) {
    // Durumu "İşaretli" (2) yap
    const updatedTask = { ...task, state: 2 };
    // Değişikliği backend'e kaydetmek için updateTask'ı çağır
    await updateTask(updatedTask);
  }
}

// Bu fonksiyon, değişikliği backend'e gönderir.
async function updateTask(task) {
  const res = await fetch(`/api/todos/${task.id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(task)
  });

  if (res.ok) {
    const updatedUser = await res.json();
    user.value = updatedUser;
    fetchTasks();
  } else {
    alert("Görev güncellenemedi.");
  }
}

function selectCategory(category) {
  activeCategory.value = category;
}

function selectPopupCategory(category) {
  newTaskCategory.value = category;
}

// --- İZLEYİCİLER (WATCHERS) ---

// Kategori değiştiğinde görevleri yeniden getirir.
// { immediate: true } sayesinde sayfa ilk yüklendiğinde de çalışır.
watch(activeCategory, fetchTasks, { immediate: true });

// Takvimde seçili tarih değiştiğinde görevleri yeniden getirir.
watch(selectedDate, (newValue, oldValue) => {
  if (newValue.toDateString() !== oldValue.toDateString()) {
    if (activeCategory.value === 'Günlük') {
      fetchTasks();
    }
  }
});

function showDetails(task) {
  // formatYerelTarih fonksiyonunu kullanarak tarihi formatlayıp uyarıda gösteriyoruz
  alert(`Oluşturulma Zamanı:\n${formatYerelTarih(task.createdAt)}`);
}

// --- YAŞAM DÖNGÜSÜ VE OLAY DİNLEYİCİLERİ ---

// MEVCUT handleClickOutside FONKSİYONUNU SİL VE BUNU YAPIŞTIR

const handleClickOutside = (event) => {
  const isClickOnDarkModeButton = event.target.closest('.theme-toggle-button');
  if (isClickOnDarkModeButton) return;

  // Hangi panelin aktif olduğunu ve referanslarını tek bir yerden kontrol edelim
  let activePanelRef = null;
  let activeButtonRef = null;
  let visibilityFlag = null;

  if (activeCategory.value === 'Günlük') {
    activePanelRef = calendarContainerRef;
    activeButtonRef = calendarButtonRef;
    visibilityFlag = isCalendarVisible;
  } else if (activeCategory.value === 'Haftalık') {
    activePanelRef = weekSelectorPanelRef;
    activeButtonRef = weekSelectorButtonRef;
    visibilityFlag = isWeekSelectorVisible;
  } else if (activeCategory.value === 'Aylık') {
    activePanelRef = monthSelectorPanelRef;
    activeButtonRef = monthSelectorButtonRef;
    visibilityFlag = isMonthSelectorVisible;
  }

  // Eğer aktif bir panel varsa ve görünür durumdaysa kontrolü yap
  if (visibilityFlag && visibilityFlag.value) {
    // Optional chaining (?.) kullanarak referansların null olma ihtimaline karşı kodu daha güvenli hale getirdik.
    const isClickOnButton = activeButtonRef.value?.contains(event.target);
    const isClickInPanel = activePanelRef.value?.contains(event.target);

    if (!isClickOnButton && !isClickInPanel) {
      visibilityFlag.value = false;
    }
  }
};

// onMounted ve onUnmounted (BİRLEŞTİRİLDİ VE TEMİZLENDİ)
onMounted(() => {
  fetchUser(); // YENİ: Bileşen yüklendiğinde kullanıcı bilgilerini de getir
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});

function formatYerelTarih(isoString) {
  if (!isoString) return '';
  const tarih = new Date(isoString);

  return tarih.toLocaleString('tr-TR'); 
}
// ESKİ getCurrentWeekNumber FONKSİYONUNU SİLİP BUNU YAPIŞTIR

function getCurrentWeekNumber() {
  const d = new Date();
  // Tarihin bir kopyasını oluştur, orijinalini değiştirme
  const date = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
  // Haftanın Perşembe gününü bul (ISO 8601 standardı bunu gerektirir)
  date.setUTCDate(date.getUTCDate() + 4 - (date.getUTCDay() || 7));
  // Yılın ilk gününü bul
  const yearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1));
  // İki tarih arasındaki gün farkını hesapla, 7'ye böl ve yukarı yuvarla
  const weekNo = Math.ceil((((date - yearStart) / 86400000) + 1) / 7);
  return weekNo;
}
function selectWeek(week) {
  selectedWeek.value = week;
  fetchTasks(); // Yeni haftanın görevlerini getir
}

function prevYear() {
  selectedYear.value--;
  fetchTasks(); // Yeni yılın görevlerini getir
}

function nextYear() {
  selectedYear.value++;
  fetchTasks(); // Yeni yılın görevlerini getir
}
function selectMonth(monthIndex) {
  selectedMonthDate.value = new Date(selectedMonthDate.value.setMonth(monthIndex));
  fetchTasks();
}

function prevYearForMonthView() {
  const currentYear = selectedMonthDate.value.getFullYear();
  selectedMonthDate.value = new Date(selectedMonthDate.value.setFullYear(currentYear - 1));
  fetchTasks();
}

function nextYearForMonthView() {
  const currentYear = selectedMonthDate.value.getFullYear();
  selectedMonthDate.value = new Date(selectedMonthDate.value.setFullYear(currentYear + 1));
  fetchTasks();
}
const totalDailyScore = computed(() => {
  if (!user.value) return 0; 
  return (user.value.dailyTasksCompleted * 2) - (user.value.dailyTasksFailed * 3);
});

const totalWeeklyScore = computed(() => {
  if (!user.value) return 0;
  // (Başarılı * 7) - (Başarısız * 10)
  return (user.value.weeklyTasksCompleted * 7) - (user.value.weeklyTasksFailed * 10);
});

const totalMonthlyScore = computed(() => {
  if (!user.value) return 0;
  // (Başarılı * 25) - (Başarısız * 35)
  return (user.value.monthlyTasksCompleted * 25) - (user.value.monthlyTasksFailed * 35);
});

function handleMouseEnter(category) {
  // Eğer başka bir zamanlayıcı varsa önce onu temizle
  clearTimeout(detailsTimer.value);
  detailsTimer.value = setTimeout(() => {
    visibleDetails.value = category;
  }, 200);
}

function handleMouseLeave() {
  visibleDetails.value = null;
  clearTimeout(detailsTimer.value);
}

function logout() {
  // 1. Tarayıcının hafızasındaki kullanıcı ID'sini sil
  localStorage.removeItem('userId');
  // 2. Kullanıcıyı ana sayfaya (veya giriş sayfasına) yönlendir
  window.location.href = '/';
}
</script>

<style>
.calendar-container .vc-day.is-today:not(:has(.vc-highlights)) .vc-day-content {
  background: var(--kutu-arkaplan);
  font-weight: 700 !important;
}

.calendar-container .vc-container {
  
  /* Takvimin genel arkaplan rengi */
  --vc-bg: var(--arka-plan); /* Örnek: Koyu Gri-Mavi */

  /* Normal günlerin ve genel yazıların rengi */
  --vc-color: var(--yazi-rengi);

  --vc-weekday-color:var(--yazi-rengi); 
  
}
.calendar-container .vc-arrow {
  background-color: var(--buton-arkaplan);
  color: var(--kutu-arkaplan);
  border-radius: 16px;
}
.calendar-container .vc-arrow:hover {
  background-color: var(--buton-hover-arkaplan);
}
.calendar-container .vc-title {
  background-color: var(--buton-arkaplan);
  color: var(--kutu-arkaplan);
}
.calendar-container .vc-title:hover {
  background-color: var(--buton-hover-arkaplan);
}

.vc-nav-popover-container {
  background-color: var(--arka-plan);
  border: none;
}

.vc-nav-title {
  color: var(--kutu-arkaplan);
  background-color: var(--buton-arkaplan);
}

.vc-nav-title:hover {
  background-color: var(--buton-hover-arkaplan);
}

.vc-nav-arrow {
  color: var(--kutu-arkaplan);
  background-color: var(--buton-arkaplan);
}
.vc-nav-arrow:hover {
 background-color: var(--buton-hover-arkaplan);
}

.vc-nav-item {
  background-color: var(--kutu-arkaplan);
  color: var(--yazi-rengi);
}
.vc-nav-item:hover {
  background-color: var(--buton-hover-arkaplan);
}
.vc-nav-item.is-focused {
  background-color: var(--arka-plan);
}
.vc-nav-item.is-active {
  background-color: var(--input-yazi);
  color: var(--buton-yazi);
}
</style>

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

.sidebar-toggle-container {
  margin-top: auto; /* Panelin en altına iter */
  padding-top: 2em; /* Üstündeki elemanlarla boşluk bırakır */
}

.sidebar-toggle-button {
  background: var(--kutu-arkaplan);
  border: none;
  color: var(--buton-yazi);
  cursor: pointer;
  padding: 10px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s ease;
}

.sidebar-toggle-button:hover {
  background-color: rgba(254, 227, 19, 1);
}

.sidebar-toggle-button svg {
  stroke: var(--buton-yazi);
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
  display: flex; 
  flex-direction: column;
}

.todos-panel-header { 
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1em;
}

.calendar-toggle-button {
    background: transparent;
    border: none;
    color: var(--yazi-rengi);
    cursor: pointer;
    padding: 5px;
    border-radius: 50%;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background-color 0.2s ease;
}

.calendar-toggle-button:hover {
    background-color: var(--input-arkaplan);
}

.calendar-container { 
    margin-bottom: 2em;
    border: 1px solid var(--input-arkaplan);
    border-radius: 8px;
    padding: 10px;
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
  justify-content: space-between; /* <-- YENİ EKLENDİ: Öğeleri iki yana yaslar */
  padding: 1em;
  border-bottom: 1px solid #eee;
  gap: 1em; /* <-- YENİ EKLENDİ: Metin ve checkbox arasına boşluk bırakır */
}
.todo-list li.completed span {
  text-decoration: line-through;
  opacity: 0.6;
}
.todo-list li input[type="checkbox"] {
  margin-left: 1em;
  width: 20px;
  height: 20px;
  -webkit-appearance: none; /* Varsayılan tarayıcı stilini kaldırır */
  -moz-appearance: none;    /* Varsayılan tarayıcı stilini kaldırır */
  appearance: none;         /* Varsayılan tarayıcı stilini kaldırır */
  border: 2px solid var(--arka-plan); /* Dış çizgi rengini temaya göre ayarla */
  border-radius: 4px;       /* Hafif yuvarlak köşeler */
  background-color:var(--selected-checkbox-bg) ; /* İç başlangıç rengini şeffaf yapar */
  cursor: pointer;          /* Fare üzerine geldiğinde imleci değiştirir */
  transition: background-color 0.2s ease, border-color 0.2s ease; /* Yumuşak geçişler */
}
.todo-list li input[type="checkbox"]:checked {
  background-color:transparent;
}

/* İşaretli kutucuğun içindeki tik işareti için (isteğe bağlı, daha gelişmiş) */
.todo-list li input[type="checkbox"]:checked::before {
  content: '✓';
  display: block;
  font-size: 32px;
  line-height: 0.1;
  text-align: center;
  color: green;
}
.todo-list li span {
  flex-grow: 1;
}

.task-title {
  flex-grow: 1;
}

/* Sağdaki butonları gruplayan konteyner */
.task-actions {
  display: flex;
  align-items: center;
  gap: 8px; /* Butonlar arasına boşluk koyar */
}
.delete-button {
  background: none;
  border: none;
  color: #e74c3c;
  font-size: 32px;
  cursor: pointer;
  visibility: hidden;
  opacity: 0;
  transition: opacity 0.2s ease;
}
.todo-list li:hover .details-button,
.todo-list li:hover .delete-button {
  visibility: visible;
  opacity: 1;
}
.details-button {
  background: none;
  border: none;
  color: var(--yazi-rengi);
  font-size: 25px;
  font-weight: bold;
  cursor: pointer;
  padding: 0 5px;
  visibility: hidden; 
  opacity: 0;
  transition: opacity 0.2s ease;
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

.week-selector-panel {
  background-color: var(--arka-plan);
  border-radius: 8px;
  padding: 1em;
  margin-bottom: 2em;
}

.year-navigator {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1em;
}

.year-navigator h3 {
  margin: 0;
  font-weight: 500;
}

.year-navigator button {
  background: none;
  border: none;
  font-size: 2em;
  color: var(--yazi-rengi);
  cursor: pointer;
  padding: 0 10px;
}

.week-grid {
  display: grid;
  grid-template-columns: repeat(13, 1fr);
  gap: 10px;
}

.week-item {
  padding: 10px;
  text-align: center;
  border-radius: 6px;
  cursor: pointer;
  background-color: var(--input-arkaplan);
  transition: background-color 0.2s ease, color 0.2s ease;
}

.week-item:hover {
  background-color: var(--buton-arkaplan-hover);
  color: var(--buton-yazi);
}

.week-item.selected {
  background-color: var(--buton-arkaplan);
  color: var(--buton-yazi);
  font-weight: bold;
  border-radius: 10px;
}

.week-selector-panel .year-navigator h3 {
    color: var(--kutu-arkaplan);
}
.week-selector-panel .year-navigator {
    background-color: var(--buton-arkaplan);
    border-radius: 5px;
}

/* Yıl oklarının rengini değiştirir */
.week-selector-panel .year-navigator button {
    color: var(--kutu-arkaplan);
}

/* İçinde bulunduğumuz haftayı (mevcut hafta) sarı ile vurgular */
.week-selector-panel .week-item.current {
  background-color: var(--kutu-arkaplan);
  color: #111827;
}

.week-selector-panel .week-item.selected {
  background-color: var(--buton-arkaplan);
  color: var(--buton-yazi);
  font-weight: bold;
}

.month-selector-panel {
  color: var(--yazi-rengi);
  border-radius: 25px;
  padding: 1em;
  margin-bottom: 2em;
  border-radius: 0;
  border: 2px dashed var(--arka-plan);
}

.month-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1em;
}

.month-item {
  padding: 15px 10px;
  text-align: center;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.month-item:hover {
  color: var(--arka-plan);
  font-weight: bold;
}

.month-selector-panel .year-navigator {
  background-color: var(--arka-plan);
  padding: 10px;
  border-radius: 6px;
  margin-bottom: 15px;
}

.month-selector-panel .year-navigator h3 {
    color: var(--kutu-arkaplan);
}

.month-selector-panel .year-navigator button {
    color: var(--kutu-arkaplan);
}

.month-selector-panel .month-item.current {
  background-color: var(--yazi-rengi);
  color: var(--arka-plan); 
  font-weight: bold;
}

.month-selector-panel .month-item.selected {
  background-color: var(--kutu-arkaplan);
  color: var(--yazi-rengi);
  border-radius: 0;
  border: 2px dashed var(--arka-plan);  
}

.todo-list li.disabled {
  opacity: 0.5;
}

/* Temaya uygun soluk renk */
:root.dark .todo-list li.disabled {
    background-color: #2d3748;
}
.loading-spinner {
  display: flex;
  position: relative;
  width: 80px;
  height: 80px;
  margin: 50px auto;
}
.loading-spinner div {
  box-sizing: border-box;
  display: block;
  position: absolute;
  width: 64px;
  height: 64px;
  margin: 8px;
  border: 8px solid var(--yazi-rengi);
  border-radius: 50%;
  animation: loading-spinner 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
  border-color: var(--yazi-rengi) transparent transparent transparent;
}
.loading-spinner div:nth-child(1) { animation-delay: -0.45s; }
.loading-spinner div:nth-child(2) { animation-delay: -0.3s; }
.loading-spinner div:nth-child(3) { animation-delay: -0.15s; }
@keyframes loading-spinner {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0,0,0, 0.6);
  z-index: 999;
  transition: opacity 0.3s ease;
}

.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  width: 300px;
  height: 100vh;
  background-color: var(--kutu-arkaplan);
  color: var(--yazi-rengi);
  padding: 2em;
  z-index: 1000; /* Her şeyin en üstünde */
  display: flex;
  flex-direction: column;
  transform: translateX(0); 
  justify-content: center;
  align-items: center;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2em;
  border-bottom: 1px solid var(--input-arkaplan);
  padding-bottom: 1em;
}

.sidebar-header h3 {
  margin: 0;
}

.close-sidebar-button {
  background: none;
  border: none;
  color: var(--yazi-rengi);
  font-size: 2rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
}

.user-profile {
  width: 100%;
  height: 100%;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 4em;
}

.task-stats {
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  gap: 15px; 
}

.stat-button {
  background-color: var(--kutu-arkaplan);
  color: var(--buton-yazi);
  padding: 18px 15px;
  border: 1px solid var(--yazi-rengi);
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  font-size: 1.2em;
  transition: all 0.2s ease;
  width: 100%;
  box-shadow: 0 5px 5px rgba(0,0,0,0.2);
  position: relative;
  z-index: 10;
}

.stat-button:hover {
  transform: translateY(-12px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}

.stat-button span {
  font-size: 0.8em;
  margin-top: 5px;
}

.score-container {
  width: 100%;
  display: flex;
  justify-content: center;
}

.score-button {
  background-color: var(--kutu-arkaplan);
  color: var(--buton-yazi);
  border: 1px solid var(--yazi-rengi);
  border-radius: 20px;
  padding: 12px 30px;
  font-size: 1.2em;
  font-weight: bold;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 5px 4px rgba(0,0,0,0.2);
}

.score-button:hover {
  transform: scale(1.08);
  box-shadow: 0 3px 4px rgba(0,0,0,0.4);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateX(-100%);
}

.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: transform 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94);
}

.todo-list li.failed {
  opacity: 0.7; 
}

.todo-list li.failed span {
  text-decoration: line-through; 
  text-decoration-color: rgba(231, 76, 60, 0.8); 
}

.todo-list li.failed input[type="checkbox"]::before {
  content: '✗'; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  width: 100%; 
  height: 70%;
  font-size: 35px;
  line-height: 1; 
  color: #e74c3c;
  box-sizing: border-box;
}

.todo-list li.failed input[type="checkbox"] {
  background-color: transparent;
}

.stat-item-wrapper {
  position: relative;
}

.details-panel {
  background-color: var(--arka-plan); 
  border-radius: 8px;
  padding: 15px;
  margin-top: -20px;
  border: 1px solid var(--input-arkaplan);
  overflow: hidden;
  border-top-left-radius: 0;  
  border-top-right-radius: 0;
  border-top: none;          
}

.stat-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9em;
  padding: 4px 0;
}

.stat-line span:first-child {
  opacity: 0.8;
}

.text-success {
  color: #2ecc71;
  font-weight: bold;
}

.text-danger {
  color: #e74c3c;
  font-weight: bold;
}

.details-slide-enter-active{
  transition: max-height 0.4s ease-in-out, 
              opacity 0.4s ease-out 0.2s, 
              padding-top 0.4s ease-in-out,
              padding-bottom 0.4s ease-in-out,
              margin-top 0.4s ease-in-out;
  max-height: 200px;
}

.details-slide-leave-active {
  transition: max-height 0.4s ease-in-out, 
              opacity 0.3s ease-in,
              padding-top 0.4s ease-in-out,
              padding-bottom 0.4s ease-in-out,
              margin-top 0.4s ease-in-out;
  max-height: 200px;
}

.details-slide-enter-from,
.details-slide-leave-to {
  opacity: 0;
  max-height: 0;
  padding-top: 0;
  padding-bottom: 0;
  margin-top: 0;
}

.sidebar-actions {
  padding-top: 2em;
  width: 100%;
  display: flex;
  gap: 10px;
}

.logout-button,
.delete-account-button {
  border: none;
  padding: 6px 60px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.logout-button {
  background-color: var(--arka-plan);
  color: var(--yazi-rengi);
  padding: 8px;
  border: 1px solid var(--yazi-rengi);
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  font-size: 1.2em;
  transition: all 0.2s ease;
  width: 100%;
  box-shadow: 0 5px 5px rgba(0,0,0,0.2);
}

.logout-button:hover {
  background-color: var(--selected-buton-yazi);
  color: var(--selected-buton-arkaplan);
  transform: scale(0.9);
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}

.delete-account-button {
  background-color: var(--arka-plan);
  color: var(--yazi-rengi);
  padding: 8px;
  border: 1px solid var(--yazi-rengi);
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  font-size: 1.2em;
  transition: all 0.2s ease;
  width: 100%;
  box-shadow: 0 5px 5px rgba(0,0,0,0.2);
}

.delete-account-button:hover {
  background-color: #ab1303ff; 
  transform: scale(0.9);
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
  color: var(--selected-buton-arkaplan);
  border: 1px solid #ab1303ff;
}

/* Bu bloğu <style scoped> etiketinin içine ekle */

.sidebar-toggle-container {
  margin-top: auto; /* Zaten vardı, en alta itmeye devam ediyor */
  padding-top: 2em;
}

.bottom-left-actions {
  display: flex; /* Butonları yan yana getirir */
  align-items: center;
  gap: 15px; /* Aralarına boşluk koyar */
}

.leaderboard-toggle-button {
  background: var(--buton-arkaplan);
  color: var(--buton-yazi);
  border: 3px solid var(--buton-arkaplan);
  cursor: pointer;
  height: 44px;
  padding: 0 80px;
  border-radius: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s ease;
  font-size: 24px;      
  line-height: 1;
}

.leaderboard-toggle-button:hover {
  background-color: var(--arka-plan);
  color: var(--yazi-rengi);
  border: 3px solid var(--yazi-rengi);
}

</style>
