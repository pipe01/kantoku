import "babel-polyfill"
import { browser } from "webextension-polyfill-ts";
import md5 from "md5";

import { fetchInfo } from "./fetchers"
import { Events } from "./shared";

document.querySelectorAll("video").forEach(el => attachTo(<Element>el));

var observer = new MutationObserver((mutations) => {
    for (const mut of mutations) {
        if (mut.type == "childList")
            mut.addedNodes.forEach(el => attachTo(<Element>el));
    }
});

observer.observe(document, { attributes: false, childList: true, characterData: false, subtree:true });

function debug(...msg: any[]) {
    if (process.env.NODE_ENV == "development") {
        console.log(...msg);
    }
}

function attachTo(element: Element) {
    if (element.tagName != "VIDEO")
        return;

    var id: string | null;

    var video = <HTMLVideoElement>element
    var keepalive: NodeJS.Timeout;

    function sendMessage(ev: Events, data?: any) {
        if (!id)
            return;
    
        debug("send message", ev, data)
        browser.runtime.sendMessage([ev, id, ...(data ? [data] : [])])
    }

    function close() {
        if (id) {
            sendMessage(Events.Closed);
            id = null;
        }
    }

    async function started() {
        close();
        
        const media = await fetchInfo();
        if (!media) {
            debug("failed to fetch media info");
            return;
        }
        if (!media.duration) {
            media.duration = video.duration;
        }

        var date = new Date();
        id = md5(date.toString() + date.getMilliseconds());

        sendMessage(Events.Started, media);
        keepalive = setInterval(() => sendMessage(Events.Keepalive), 5000);

        if (!video.paused)
            sendMessage(Events.Resumed);
    }

    window.addEventListener("beforeunload", e => close());

    video.addEventListener("pause", () => sendMessage(Events.Paused));
    video.addEventListener("play", () => sendMessage(Events.Resumed));
    video.addEventListener("timeupdate", () => sendMessage(Events.TimeUpdated, [video.currentTime, video.duration]));
    video.addEventListener("loadedmetadata", started);
}
