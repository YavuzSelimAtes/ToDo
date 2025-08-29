import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import Todo from '../views/ToDo.vue'

const routes = [
  { path: '/', redirect: '/login' },       
  { path: '/login', component: Login },
  { path: '/todo', component: Todo }
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})
