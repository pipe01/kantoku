import { inject, provide, reactive } from "vue";
import * as models from "./models";

const key = "api";

export function provideApi() {
    const ws = new WebSocket(process.env.NODE_ENV == "development" ? `ws://192.168.1.33:4545/ws` : `ws://${location.host}/ws`);
    const api = new ApiClient(ws);

    provide(key, api);
    return api;
}

export function useApi() {
    return inject<ApiClient>(key);
}

class ApiClient {
    sessions: { [id: string]: models.Session } = reactive({});
    private ws: WebSocket;

    constructor(ws: WebSocket) {
        this.ws = ws;

        ws.addEventListener("message", msg => this.handleMessage(msg));
    }

    private handleMessage(msg: MessageEvent) {
        const ev: models.Event = JSON.parse(msg.data);

        switch (ev.kind) {
            case models.EventKind.SessionStart:
            {
                const session = ev.data as models.Session;
                
                this.sessions[session.id] = reactive(session);
                break;
            }

            case models.EventKind.SessionEnd:
                delete this.sessions[ev.data];
                break;

            case models.EventKind.SessionUpdated:
            {
                const data = ev.data as models.Session;
                
                const session = this.sessions[ev.data.id];
                const icon = session.app.icon;
                
                Object.assign(session, ev.data);

                session.app.icon = icon;
                break;
            }
        }
    }

    private sendMessage(kind: models.EventKind, data?: any) {
        const ev: models.Event = { kind }
        if (data) {
            ev.data = data;
        }

        this.ws.send(JSON.stringify(ev))
    }

    public pause() {
        this.sendMessage(models.EventKind.Pause);
    }

    public play() {
        this.sendMessage(models.EventKind.Play);
    }

    public stop() {
        this.sendMessage(models.EventKind.Stop);
    }

    public previous() {
        this.sendMessage(models.EventKind.Previous);
    }

    public next() {
        this.sendMessage(models.EventKind.Next);
    }

    public setPosition(pos: number) {
        this.sendMessage(models.EventKind.SetPosition, pos);
    }
}