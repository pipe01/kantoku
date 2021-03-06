export enum EventKind {
    SessionStart,
    SessionEnd,
    SessionUpdated,

    Pause,
    Play,
    Stop,
    Previous,
    Next,
    SetPosition,
}

export interface Event {
    kind: EventKind;
    data?: any;
    session?: string;
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
