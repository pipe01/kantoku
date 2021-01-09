<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.blue-back
        .hero-body
            .container
                h1.title.has-text-light Kantoku

    .sessions.is-flex-grow-1.is-flex.is-align-items-center
        div(v-for="session in sessions")
            Session(:session="session")
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

        return { sessions: api.sessions }
    }
})
</script>

<style lang="scss" scoped>
.blue-back {
    background: #1561C2;
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