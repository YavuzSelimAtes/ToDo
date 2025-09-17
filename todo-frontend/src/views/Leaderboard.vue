<template>
  <transition name="panel-slide">
    <div v-if="leaderboardData" class="leaderboard-container">
      <div class="leaderboard-overlay" @click="close"></div>

      <div class="leaderboard-panel">
        <div class="leaderboard-header">
          <h2>Liderlik Tablosu</h2>
          <button @click="close" class="close-button">&times;</button>
        </div>

        <div class="leaderboard-content">
          <ol class="leaderboard-list">
            <li v-for="(player, index) in leaderboardData.topUsers" :key="player.username"
                :class="{ 'current-user': player.username === leaderboardData.currentUserRank.username }">
              <span class="rank">{{ index + 1 }}</span>
              <span class="username">{{ player.username }}</span>
              <span class="score">{{ player.score }} Puan</span>
            </li>
          </ol>

          <div v-if="shouldShowCurrentUserRank" class="current-user-rank-banner">
            <span class="rank">{{ leaderboardData.currentUserRank.rank }}</span>
            <span class="username">{{ leaderboardData.currentUserRank.username }} (Siz)</span>
            <span class="score">{{ leaderboardData.currentUserRank.score }} Puan</span>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

// --- PROPS VE EMITS ---
const props = defineProps({
  userId: {
    type: [String, Number],
    required: true
  }
});
const emit = defineEmits(['close']);


// --- DEĞİŞKENLER ---
const leaderboardData = ref(null); // Başlangıçta null olması önemli
const isLoading = ref(true);


// --- FONKSİYONLAR ---
async function fetchLeaderboard() {
  if (!props.userId) return;
  isLoading.value = true;
  try {
    const res = await fetch(`http://localhost:5000/api/leaderboard/${props.userId}`);
    if (res.ok) {
      leaderboardData.value = await res.json();
    } else {
      console.error("Liderlik tablosu verisi alınamadı.");
    }
  } catch (error) {
    console.error("Liderlik tablosu alınırken hata:", error);
  } finally {
    isLoading.value = false;
  }
}

const shouldShowCurrentUserRank = computed(() => {
  if (!leaderboardData.value) return false;
  return leaderboardData.value.currentUserRank.rank > 100;
});

function close() {
  emit('close');
}

// Component ekrana geldiğinde veriyi çek
onMounted(() => {
  fetchLeaderboard();
});
</script>

<style scoped>
.leaderboard-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1000;
  display: flex; /* Paneli sağa yaslamak için */
  justify-content: flex-end; /* İçeriği sağa yasla */
}

.leaderboard-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
}

.leaderboard-panel {
  width: 100%;
  max-width: 400px; /* Panel genişliği */
  height: 100%;
  background: var(--arka-plan);
  color: var(--yazi-rengi);
  box-shadow: -5px 0 15px rgba(0,0,0,0.3);
  display: flex;
  flex-direction: column;
  position: relative; /* Overlay'in üzerinde kalması için */
  z-index: 1001;
}

.leaderboard-header {
  padding: 1.5em;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--input-arkaplan);
  flex-shrink: 0; /* Küçülmesini engelle */
}

.leaderboard-header h2 {
  margin: 0;
}

.close-button {
  background: none;
  border: none;
  font-size: 2.5rem;
  color: var(--yazi-rengi);
  cursor: pointer;
  line-height: 1;
}

.leaderboard-content {
  padding: 1.5em;
  overflow-y: auto; /* Liste uzunsa kaydırma çubuğu çıksın */
}

.leaderboard-list { 
    list-style: none; 
    padding: 0; 
    margin: 0; 
}

.leaderboard-list li { 
    display: flex; 
    align-items: center; 
    padding: 12px 8px; 
    border-radius: 6px; 
    transition: 
    background-color 0.2s ease; 
    border-radius: 25px;
    border: 2px solid var(--kutu-arkaplan);
    margin-bottom: 2px;
}

.leaderboard-list li:nth-child(odd) { 
    background-color: var(--input-arkaplan); 
}

.leaderboard-list li.current-user { 
    background-color: var(--kutu-arkaplan); 
    font-weight: bold; 
    border-radius:10px;
    border: 2px solid var(--yazi-rengi); 
}

.rank { 
    font-weight: bold; 
    font-size: 1.1em; 
    width: 40px; 
    text-align: center; 
    color: var(--yazi-rengi); 
    opacity: 0.8; 
}

.username { 
    flex-grow: 1; 
    padding: 0 1em; 
}

.score { 
    font-weight: bold; 
}

.current-user-rank-banner { 
    margin-top: 1.5em; 
    padding-top: 1.5em; 
    border-top: 2px dashed var(--yazi-rengi); 
    display: flex; 
    align-items: center; 
    font-weight: bold; 
}

/* --- SAĞDAN KAYMA ANİMASYONU --- */
.panel-slide-enter-active,
.panel-slide-leave-active {
  transition: all 0.4s ease-in-out;
}

.panel-slide-enter-active .leaderboard-overlay,
.panel-slide-leave-active .leaderboard-overlay {
  transition: opacity 0.4s ease-in-out;
}

.panel-slide-enter-active .leaderboard-panel,
.panel-slide-leave-active .leaderboard-panel {
  transition: transform 0.4s ease-in-out;
}

.panel-slide-enter-from,
.panel-slide-leave-to {
  opacity: 0;
}

.panel-slide-enter-from .leaderboard-panel,
.panel-slide-leave-to .leaderboard-panel {
  transform: translateX(100%); /* Paneli ekranın sağına iter */
}
</style>