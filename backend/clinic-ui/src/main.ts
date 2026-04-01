import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

// Bootstrap Framework
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import 'bootstrap-icons/font/bootstrap-icons.css'

// Global Design System Styles
import './styles/index.css'

const app = createApp(App)

const pinia = createPinia()

app.use(pinia)    
app.use(router) 

app.mount('#app')
