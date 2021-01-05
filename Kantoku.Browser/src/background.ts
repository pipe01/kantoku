import { browser } from "webextension-polyfill-ts";

//On startup, connect to satellite.
var port = browser.runtime.connectNative("kantoku");

// Listen for messages from satellite.
port.onMessage.addListener((response) => {
    console.log("Received: " + response);
});

// browser.browserAction.onClicked.addListener(() => {
//     console.log("Sending:  ping");
//     port.postMessage({nice: 123});
// });

browser.runtime.onMessage.addListener(msg => {
    console.log(msg);
    
    if (Array.isArray(msg) && (msg.length == 2 || msg.length == 3))
        port.postMessage(msg);
});