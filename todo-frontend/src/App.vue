<template>
  <div id="app">
    <!-- Tema değiştirme düğmesi -->
    <button @click.stop="toggleTheme" class="theme-toggle-button">
      <!-- isDarkMode durumuna göre Güneş veya Ay ikonu gösterir -->
      <span v-if="isDarkMode">☀️</span>
      <span v-else>🌙</span>
    </button>

    <!-- Diğer sayfalar (Login, Todo vb.) burada gösterilir -->
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
// Oluşturduğumuz tema yöneticisinden gerekli fonksiyonları ve değişkeni import ediyoruz.
import { isDarkMode, toggleTheme, initTheme } from './themeStore.js';

// Uygulama ilk yüklendiğinde, kaydedilmiş temayı uygulamak için initTheme fonksiyonunu çağırıyoruz.
onMounted(() => {
  initTheme();
});
</script>

<style>

.theme-toggle-button {
  position: fixed; /* Sayfa kaysa bile yerinde kalır */
  top: 20px;
  right: 20px;
  background-color: var(--tema-dugme-arkaplan);
  color: var(--yazi-rengi);
  border: 1px solid #ccc;
  width: 50px;
  height: 50px;
  border-radius: 50%; /* Yuvarlak yapar */
  cursor: pointer;
  z-index: 1001; /* Diğer elemanların önünde olmasını sağlar */
  font-size: 24px;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: background-color 0.3s ease, color 0.3s ease;
}
</style>
