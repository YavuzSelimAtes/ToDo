<template>
  <div>
    <h1 class="baslik-yazisi">Welcome Don'ts</h1>
  </div>
  <div class="auth-container">
    <div class="auth-flipper-container" :class="{ 'show-register': isRegisterVisible }">
      <div class="auth-flipper">

        <div class="auth-panel login-panel">
          <div class="tema-kutusu">
            <h1>Giriş Yap</h1>
            <form @submit.prevent="login">
              <input v-model="username" placeholder="Kullanıcı Adı" />
              <input v-model="password" type="password" placeholder="Şifre" />
              <button type="submit" class="tema-butonu">Giriş Yap</button>
            </form>
            <p>
              Hesabın yoksa
              <a href="#" @click.prevent="isRegisterVisible = true">Kaydol</a>
            </p>
          </div>
        </div>

        <div class="auth-panel register-panel">
          <div class="tema-kutusu">
            <a href="#" class="back-arrow" @click.prevent="isRegisterVisible = false">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor"><path d="M19 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H19v-2z"/></svg>
            </a>
            <h1>Kaydol</h1>
            <form @submit.prevent="register">
              <input v-model="regUsername" placeholder="Kullanıcı Adı" />
              <input v-model="regPassword" type="password" placeholder="Şifre" />
              <button type="submit" class="tema-butonu">Kayıt Ol</button>
            </form>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const isRegisterVisible = ref(false);

const username = ref('');
const password = ref('');
const router = useRouter();

const regUsername = ref('');
const regPassword = ref('');

async function login() {
  // --- GÜVENLİK KONTROLÜ EKLENDİ ---
  if (!username.value.trim() || !password.value.trim()) {
    alert('Kullanıcı adı ve şifre alanları boş bırakılamaz!');
    return; // Fonksiyonun devam etmesini engelle
  }

  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: username.value, password: password.value })
  });
  if (res.ok) {
    const data = await res.json();
    localStorage.setItem('userId', data.userId);
    router.push('/todo');
  } else {
    alert('Giriş başarısız!');
  }
}

async function register() {
  // --- GÜVENLİK KONTROLÜ EKLENDİ ---
  if (!regUsername.value.trim() || !regPassword.value.trim()) {
    alert('Kullanıcı adı ve şifre alanları boş bırakılamaz!');
    return;
  }

  const res = await fetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: regUsername.value, password: regPassword.value })
  });
  if (res.ok) {
    alert('Kayıt başarılı, şimdi giriş yapabilirsiniz!');
    isRegisterVisible.value = false;
  } else {
    alert('Kayıt başarısız!');
  }
}
</script>

<style scoped>
.baslik-yazisi {
  font-family: 'Playfair Display', serif;
  color: #e3bd10ff;
  font-size: 5rem; 
  text-align: center;
}
.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100vw;
}

.auth-flipper-container {
  width: 460px;
  height: 520px;
  position: relative;
  overflow: hidden;
}

.auth-flipper {
  display: flex;
  width: 200%;
  position: absolute;
  top: 0;
  left: 0;
  transition: transform 0.6s ease-in-out;
}

.auth-flipper-container.show-register .auth-flipper {
  transform: translateX(-50%);
}

.auth-panel {
  width: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.tema-kutusu {
  width: 100%;
  box-sizing: border-box;
  position: relative;
}

.back-arrow {
  position: absolute;
  top: 48px;
  left: 30px;
  cursor: pointer;
  color: var(--yazi-rengi, #2c3e50);
}
</style>
