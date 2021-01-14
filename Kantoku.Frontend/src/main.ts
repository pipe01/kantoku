import { createApp } from 'vue'
import App from './App.vue'
import "./scss/style.scss";

import icons from "./icons"

import { WebStorage } from "vue-web-storage";
import router from './router'

document.addEventListener('deviceready', () => console.log("device ready"), false);

createApp(App)
    .use(router)
    .use(icons)
    .provide("storage", new WebStorage("kantoku_", "local"))
    .mount('#app')
