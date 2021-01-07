import { browser, Runtime } from "webextension-polyfill-ts";

//On startup, connect to satellite.
var port: Runtime.Port | null = connect();

function connect() {
    if (port) {
        return port;
    }
    
    console.log("connecting to satellite");
    port = browser.runtime.connectNative("kantoku");

    setError(false);
    return port
}

function setError(errored: boolean) {
    browser.browserAction.setIcon({
        path: errored ? "icons/action_error.png" : "icons/action.png"
    });
}

// Listen for messages from satellite.
port.onMessage.addListener((response) => {
    console.log("Received: ", response);
});

port.onDisconnect.addListener(() => {
    console.log("disconnected");
    port = null;
    setError(true);
});

browser.browserAction.onClicked.addListener(() => {
    connect();
});

browser.runtime.onMessage.addListener(msg => {
    if (Array.isArray(msg) && (msg.length == 2 || msg.length == 3))
        port?.postMessage(msg);
});