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

    var date = new Date();
    var id = md5(date.toString() + date.getMilliseconds());

    var video = <HTMLVideoElement>element

    function sendMessage(ev: Events, data?: any) {
        debug("send message", ev, data)
        browser.runtime.sendMessage([ev, id, ...(data ? [data] : [])])
    }

    async function started() {
        const media = await fetchInfo();
        if (!media) {
            debug("failed to fetch media info");
            return;
        }
        debug("fetched media info", media);

        sendMessage(Events.Started, media);
    }

    video.addEventListener("pause", () => sendMessage(Events.Paused));
    video.addEventListener("play", () => sendMessage(Events.Resumed));
    // video.addEventListener("timeupdate", () => sendMessage(Events.TimeUpdated, [video.currentTime, video.duration]));
    video.addEventListener("loadedmetadata", started);
}
