import { createApp } from 'vue'
import App from './App.vue'
import "./scss/style.scss";

import icons from "./icons"

import { WebStorage } from "vue-web-storage";
import router from './router'

document.addEventListener("deviceready", () => {
    chrome.sockets.udp.create(({ socketId }) => {
        chrome.sockets.udp.bind(socketId, "0.0.0.0", 54250, () => {
            console.log("socket bound");

            chrome.sockets.udp.getInfo(socketId, info => {
                console.log(`listening on ${info.localAddress}:${info.localPort}`);
            })

            chrome.sockets.udp.onReceive.addListener(rec => {
                console.log("received", rec.data);
            })
        })
    })
})

createApp(App)
    .use(router)
    .use(icons)
    .provide("storage", new WebStorage("kantoku_", "local"))
    .mount('#app')
