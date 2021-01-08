import { browser, Runtime } from "webextension-polyfill-ts";
import { Events } from "./shared";

//On startup, connect to satellite.
var port: Runtime.Port | null = connect();

var contentPorts: Runtime.Port[] = [];

var isErrored = false;

function setError(errored: boolean) {
    if (errored == isErrored)
        return;
    
    isErrored = errored;
    browser.browserAction.setIcon({
        path: errored ? "icons/action_error.png" : "icons/action.png"
    });
}
setError(true);

function connect() {
    if (port) {
        return port;
    }
    
    console.log("connecting to satellite");
    port = browser.runtime.connectNative("kantoku");

    // Listen for messages from satellite.
    port.onMessage.addListener(msg => {
        console.log("received", msg);

        if (!Array.isArray(msg))
            return;

        setError(false);
        
        for (const p of contentPorts) {
            p.postMessage(msg);
        }
    });

    port.onDisconnect.addListener(() => {
        console.log("disconnected");

        port = null;
        setError(true);
    });

    return port
}

browser.browserAction.onClicked.addListener(() => {
    connect();
});

browser.runtime.onMessage.addListener(msg => {
    connect();

    if (Array.isArray(msg) && (msg.length == 2 || msg.length == 3)) {
        port?.postMessage(msg);
    }
});

browser.runtime.onConnect.addListener(conn => {
    contentPorts.push(conn);
    conn.onDisconnect.addListener(() => contentPorts.splice(contentPorts.indexOf(conn), 1))
})