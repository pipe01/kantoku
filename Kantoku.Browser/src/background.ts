import { browser } from "webextension-polyfill-ts";

//On startup, connect to satellite.
var port = browser.runtime.connectNative("kantoku");

// Listen for messages from satellite.
port.onMessage.addListener((response) => {
    console.log("Received: " + response);
});

browser.browserAction.onClicked.addListener(() => {
    console.log("Sending:  ping");
    port.postMessage({nice: 123});
});