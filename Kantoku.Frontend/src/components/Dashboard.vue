<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.blue-back
        .hero-body
            .container.is-flex.is-align-items-center
                img(:src="connected ? '/img/kantoku.png' : '/img/kantoku_error.png'" height="70" width="70")
                span.title.has-text-light.has-text-weight-light.ml-3 Kantoku

    .sessions.is-flex-grow-1.is-flex.is-align-items-center(v-if="connected")
        div(v-for="session in sessions")
            Session(:session="session")

    .is-flex-grow-1.is-flex.is-flex-direction-column.is-justify-content-center(v-else="")
        p.has-text-centered.has-text-grey-light
            | Connecting...
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import Session from "./Session.vue";

import { provideApi } from "../api";

export default defineComponent({
    components: {
        Session
    },
    setup() {
        const api = provideApi();

        return { sessions: api.sessions, connected: api.connected }
    }
})
</script>

<style lang="scss" scoped>
.blue-back {
    background: #368DFF;
}

.sessions {
    overflow-x: scroll;
    scroll-snap-type: x mandatory;

    & > * {
        display: table-cell;
        scroll-snap-align: start;
    }
}
</style>