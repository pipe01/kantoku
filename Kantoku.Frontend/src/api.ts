import { inject, provide, reactive, ref, Ref } from "vue";
import * as models from "./models";

const key = "api";

export function provideApi(host: string) {
    const api = new ApiClient(host);

    provide(key, api);
    return api;
}

export function useApi() {
    return inject<ApiClient>(key);
}

export class ApiClient {
    sessions: { [id: string]: models.Session } = reactive({});
    connected: Ref<boolean> = ref(false);
    private expectedClose = false;
    private ws!: WebSocket;

    constructor(host: string) {
        this.connect(host);
    }

    private connect(host: string) {
        var _this = this;
        function scheduleConnect() {
            setTimeout(() => _this.connect(host), 5000);
        }

        try {
            this.clearSessions();

            this.ws = new WebSocket(`wss://${host}/ws`);

            this.ws.addEventListener("message", msg => this.handleMessage(msg));
            this.ws.addEventListener("open", () => this.connected.value = true);
            this.ws.addEventListener("close", ev => {
                this.connected.value = false;
                this.clearSessions();

                if (this.expectedClose) {
                    this.expectedClose = false;
                } else {
                    scheduleConnect();
                }
            });
        } catch (e) {
            console.error(e);
            scheduleConnect();
        }
    }

    public close() {
        this.expectedClose = true;
        this.ws.close();
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