import "babel-polyfill"
import { browser, Runtime } from "webextension-polyfill-ts";
import md5 from "md5";

import { fetchInfo } from "./fetchers"
import { Events } from "./shared";

var background: Runtime.Port | null = null;

function getBackground() {
    if (!background) {
        background = browser.runtime.connect();
    }
    return background;
}

function debug(...msg: any[]) {
    if (process.env.NODE_ENV == "development") {
        console.debug(...msg);
    }
}

class Session {
    video: HTMLVideoElement;
    keepalive: NodeJS.Timeout | null;
    id: string | null;

    constructor(video: HTMLVideoElement) {
        this.video = video;
        this.keepalive = null;
        this.id = null;
    }

    public start() {
        window.addEventListener("beforeunload", () => this.close());

        this.video.addEventListener("pause", () => this.sendMessage(Events.Paused));
        this.video.addEventListener("play", () => this.sendMessage(Events.Resumed));
        this.video.addEventListener("timeupdate", () => this.sendMessage(Events.TimeUpdated, [this.video.currentTime, this.video.duration]));
        this.video.addEventListener("loadedmetadata", () => this.started());

        getBackground().onMessage.addListener(msg => this.onBackgroundMessage(msg));

        if (this.video.readyState >= this.video.HAVE_METADATA) {
            this.started();
        }
    }

    sendMessage(ev: Events, data?: any) {
        if (!this.id)
            return;
    
        // debug("send message", ev, data);
        getBackground().postMessage([ev, this.id, ...(data ? [data] : [])]);
    }

    close() {
        if (this.id) {
            this.sendMessage(Events.Closed);
            this.id = null;

            if (this.keepalive)
                clearInterval(this.keepalive);
        }
    }

    onBackgroundMessage(msg: any) {
        if (!Array.isArray(msg) || msg.length == 0) {
            return;
        }

        if (msg[0] == Events.Keepalive) {
            this.started();
        } else if (msg.length < 2 || msg[1] != this.id) {
            return;
        }

        switch (msg[0]) {
            case Events.Play:
                this.video.play();
                break;

            case Events.Pause:
                this.video.pause();
                break;

            case Events.Previous:
                this.video.currentTime -= 10;
                break;

            case Events.Next:
                this.video.currentTime += 10;
                break;

            case Events.SetPosition:
                this.video.currentTime = msg[2];
                break;
        }
    }
    
    async started() {
        debug("started");

        // Make sure the previous session was closed
        this.close();
        
        // Fetch session media info
        const media = await fetchInfo(this.video);
        if (!media) {
            debug("failed to fetch media info");
            return;
        }
        // If the fetcher didn't provide a duration, set it to the video's duration
        if (!media.duration) {
            media.duration = this.video.duration;
        }
        debug("media info", media);

        // Generate an ID derived from the current time
        var date = new Date();
        this.id = md5(date.toString() + date.getMilliseconds());

        // Send a "started" event and start keepalive timer
        this.sendMessage(Events.Started, media);
        this.keepalive = setInterval(() => this.sendMessage(Events.Keepalive), 5000);

        // If the video is already playing, send a "resumed" event
        if (!this.video.paused)
            this.sendMessage(Events.Resumed);
    }
}

document.querySelectorAll("video").forEach(el => attachTo(<Element>el));

var observer = new MutationObserver((mutations) => {
    for (const mut of mutations) {
        if (mut.type == "childList")
            mut.addedNodes.forEach(el => attachTo(<Element>el));
    }
});

observer.observe(document, { attributes: false, childList: true, characterData: false, subtree:true });

function attachTo(element: Element) {
    if (element.tagName != "VIDEO")
        return;

    debug("attach to", element);
    
    new Session(<HTMLVideoElement>element).start();
}
