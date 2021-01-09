export enum EventKind {
    SessionStart,
    SessionEnd,
    SessionUpdated,
}

export interface Event {
    kind: EventKind;
    data: any;
}

export interface Session {
    id: string;
    position: number;
    isPlaying: boolean;
    media: SessionMedia;
    app: SessionApp;
}

export interface SessionMedia {
    title: string;
    author: string;
    duration: number;
}

export interface SessionApp {
    name: string;
    icon?: string;
}
