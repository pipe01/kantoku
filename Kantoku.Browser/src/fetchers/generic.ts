import { Fetcher } from "./";

export default <Fetcher>{
    isSupported() {
        return true;
    },
    async fetchInfo(video: HTMLVideoElement) {
        const filename = decodeURIComponent(new URL(video.currentSrc).pathname.split('/').pop() ?? "");
        
        return {
            title: filename,
            appName: "Unknown"
        }
    }
}