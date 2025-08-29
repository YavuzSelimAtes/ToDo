import { ref } from 'vue';

export const isDarkMode = ref(false);

export function toggleTheme() {
  isDarkMode.value = !isDarkMode.value;
  updateTheme();
}

function updateTheme() {
  if (isDarkMode.value) {
    document.body.classList.add('dark-mode');
    localStorage.setItem('theme', 'dark');
  } else {
    document.body.classList.remove('dark-mode');
    localStorage.setItem('theme', 'light');
  }
}

export function initTheme() {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark') {
    isDarkMode.value = true;
  }
  updateTheme();
}
