import { createApp } from 'vue'
import App from './App.vue'
import "./scss/style.scss";

import icons from "./icons"

import { WebStorage } from "vue-web-storage";
import router from './router'

createApp(App)
    .use(router)
    .use(icons)
    .provide("storage", new WebStorage("kantoku_", "local"))
    .mount('#app')
