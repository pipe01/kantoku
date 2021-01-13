<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.blue-back
        .hero-body
            .container.is-flex.is-align-items-center
                img(:src="connected ? '/img/kantoku.png' : '/img/kantoku_error.png'" height="70" width="70")
                span.title.has-text-light.has-text-weight-light.ml-3 Kantoku

    .is-flex-grow-1.is-flex.is-flex-direction-column.is-justify-content-center(v-if="!connected")
        p.has-text-centered.has-text-grey-light
            | Connecting...

    template(v-else="")
        .sessions.is-flex-grow-1.is-flex.is-align-items-center
            div(v-for="session in sessions")
                Session(:session="session" :ref="el => sessionElements[session.id] = el")

        .p-5.is-flex.is-flex-direction-row.is-justify-content-center.icon-bar
            img(v-for="session in sessions" :src="session.app?.icon" width="32" height="32")
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue';
import Session from "./Session.vue";

import { provideApi } from "../api";

export default defineComponent({
    props: {
        host: String
    },
    components: {
        Session
    },
    setup(props) {
        if (!props.host) {
            console.error("No host defined");
            return;
        }

        const api = provideApi(props.host);
        
        const sessionElements = ref([]);

        return { ...api, sessionElements }
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

.icon-bar > * {
    &:not(.active) {
        opacity: .5;
    }
    
    &:not(:last-child) {
        margin-right: 1rem;
    }
}
</style>