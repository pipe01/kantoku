import { inject, provide, reactive } from "vue";
import * as models from "./models";

const key = "api";

export function provideApi() {
    const ws = new WebSocket(process.env.NODE_ENV == "development" ? `ws://localhost:4545/ws` : `ws://${location.host}/ws`);

    provide(key, new ApiClient(ws));
}

export function useApi() {
    return inject<ApiClient>(key);
}

class ApiClient {
    sessions: { [id: string]: models.Session } = reactive({});
    ws: WebSocket;

    constructor(ws: WebSocket) {
        this.ws = ws;

        ws.addEventListener("message", msg => this.handleMessage(msg));
    }

    handleMessage(msg: MessageEvent) {
        const ev: models.Event = JSON.parse(msg.data);

        switch (ev.kind) {
            case models.EventKind.SessionStart:
                const session = ev.data as models.Session;
                
                this.sessions[session.id] = reactive(session);
                break;

            case models.EventKind.SessionEnd:
                delete this.sessions[ev.data];
                break;
        }
    }
}