import { Fetcher } from "./";

function timeout(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export default <Fetcher>{
    isSupported() {
        return /youtube\.com\/watch\?v=/.test(location.href);
    },
    async fetchInfo() {
        var titleEl: HTMLElement | null = null;
        var channelEl: HTMLElement | null = null;

        do {
            titleEl = document.querySelector(".title > yt-formatted-string.ytd-video-primary-info-renderer");
            channelEl = document.querySelector(".ytd-channel-name > a");

            await timeout(100);
        } while (!titleEl || !channelEl);
        
        return {
            title: titleEl?.innerText,
            author: channelEl?.innerText,
            appName: "YouTube"
        }
    }
}