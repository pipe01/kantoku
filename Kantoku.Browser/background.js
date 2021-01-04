/*
On startup, connect to the "ping_pong" app.
*/
var port = browser.runtime.connectNative("kantoku");

/*
Listen for messages from the app.
*/
port.onMessage.addListener((response) => {
  console.log("Received: " + response);
});

port.onDisconnect.addListener(() => {
	console.log("disconnected");
	port = null;
})

/*
On a click on the browser action, send the app a message.
*/
browser.browserAction.onClicked.addListener(() => {
  console.log("Sending:  ping");
  port.postMessage({nice: 123});
});