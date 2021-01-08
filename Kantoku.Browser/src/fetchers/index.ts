export type MediaInfo = {
    title: string;
    author?: string;
    iconUrl: string;
    appName: string;
    duration: number;
}

export interface Fetcher {
    isSupported(): boolean;
    fetchInfo(video?: HTMLVideoElement): Promise<MediaInfo>;
}

import youtube from "./youtube"
import generic from "./generic"

const fetchers = [
    youtube,
    generic,
]

export async function fetchInfo(video: HTMLVideoElement): Promise<MediaInfo | null> {
    for (const fetcher of fetchers) {
        if (fetcher.isSupported()) {
            var info = await fetcher.fetchInfo(video);

            if (!info.iconUrl) {
                info.iconUrl = "https://s2.googleusercontent.com/s2/favicons?domain_url=" + location.href;
            }

            return info;
        }
    }

    return null;
}