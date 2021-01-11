import axios from "axios";
import { Fetcher } from "./";

type EmbedData = {
    author_name: string;
    title: string;
}

function getVideoId(): string | null {
    var result = /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/\s]{11})/gi.exec(location.href);
    
    if (result) {
        return result[1];
    }
    return null;
}

export default <Fetcher>{
    isSupported() {
        return !!getVideoId();
    },
    async fetchInfo() {
        const videoId = getVideoId();

        var resp = await axios.get<EmbedData>(`https://www.youtube.com/oembed?format=json&url=https://www.youtube.com/watch?v=${videoId}`);

        return {
            title: resp.data.title,
            author: resp.data.author_name,
            appName: "YouTube"
        }
    }
}