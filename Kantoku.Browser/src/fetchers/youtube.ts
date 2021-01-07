import axios from "axios";
import { Fetcher } from "./";

type EmbedData = {
    author_name: string;
    title: string;
}

export default <Fetcher>{
    isSupported() {
        return /youtube\.com\/watch\?v=/.test(location.href);
    },
    async fetchInfo() {
        var urlParams = new URLSearchParams(location.search);
        var videoId = urlParams.get("v");
        
        var resp = await axios.get<EmbedData>(`https://www.youtube.com/oembed?format=json&url=https://www.youtube.com/watch?v=${videoId}`);

        return {
            title: resp.data.title,
            author: resp.data.author_name,
            appName: "YouTube"
        }
    }
}