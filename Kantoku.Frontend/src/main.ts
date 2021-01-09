import { createApp } from 'vue'
import App from './App.vue'
import "./scss/style.scss";

import icons from "./icons"

createApp(App)
    .use(icons)
    .mount('#app')
