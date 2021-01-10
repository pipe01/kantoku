import { inject, provide, reactive, ref, Ref } from "vue";
import * as models from "./models";

const key = "api";

export function provideApi() {
    const api = new ApiClient();

    provide(key, api);
    return api;
}

export function useApi() {
    return inject<ApiClient>(key);
}

class ApiClient {
    sessions: { [id: string]: models.Session } = reactive({});
    connected: Ref<boolean> = ref(false);
    private ws!: WebSocket;

    constructor() {
        this.connect();
    }

    private connect() {
        this.clearSessions();

        this.ws = new WebSocket(process.env.NODE_ENV == "development" ? `ws://192.168.1.33:4545/ws` : `ws://${location.host}/ws`);

        this.ws.addEventListener("message", msg => this.handleMessage(msg));
        this.ws.addEventListener("open", () => this.connected.value = true);
        this.ws.addEventListener("close", () => {
            this.connected.value = false;
            this.clearSessions();

            setTimeout(() => this.connect(), 5000);
        });
    }

    private clearSessions() {
        for (const id in this.sessions) {
            delete this.sessions[id];
        }
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
                const icon = session.app?.icon;
                
                Object.assign(session, ev.data);

                session.app.icon = icon;
                break;
            }
        }
    }

    private sendMessage(kind: models.EventKind, data?: any, session?: string) {
        const ev: models.Event = { kind }
        if (data !== undefined) {
            ev.data = data;
        }
        if (session) {
            ev.session = session;
        }

        this.ws.send(JSON.stringify(ev))
    }

    public forSession(id: string) {
        const _this = this;

        console.log(this, id);
        

        function sendMessage(kind: models.EventKind, data?: any) {
            _this.sendMessage(kind, data, id);
        }

        return {
            pause() {
                sendMessage(models.EventKind.Pause);
            },
            play() {
                sendMessage(models.EventKind.Play);
            },
            stop() {
                sendMessage(models.EventKind.Stop);
            },
            previous() {
                sendMessage(models.EventKind.Previous);
            },
            next() {
                sendMessage(models.EventKind.Next);
            },
            setPosition(pos: number) {
                sendMessage(models.EventKind.SetPosition, pos);
            }
        }
    }
}