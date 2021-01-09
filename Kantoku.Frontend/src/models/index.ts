export enum EventKind {
    SessionStart,
    SessionEnd,
}

export interface Event {
    kind: EventKind;
    data: any;
}

export interface Session {
    id: string;
}
