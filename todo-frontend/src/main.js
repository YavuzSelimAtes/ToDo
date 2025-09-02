import { createApp } from 'vue'
import './style.css' // Senin dosyadaki bu satırı koruyoruz
import App from './App.vue'
import { router } from './router' // Senin dosyadaki bu satırı da koruyoruz

// Toast kütüphanesini ve stil dosyasını import et
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'

const app = createApp(App)

app.use(router)

// Toast kütüphanesini uygulamaya kaydet
app.use(Toast, {
    transition: "Vue-Toastification__bounce",
    maxToasts: 5,
    newestOnTop: true
});

app.mount('#app')