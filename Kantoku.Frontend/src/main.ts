import { createApp } from 'vue'
import App from './App.vue'
import "./scss/style.scss";

import icons from "./icons"

import { WebStorage } from "vue-web-storage";

document.addEventListener('deviceready', () => console.log("device ready"), false);

createApp(App)
    .use(icons)
    .provide("storage", new WebStorage("kantoku_", "local"))
    .mount('#app')
