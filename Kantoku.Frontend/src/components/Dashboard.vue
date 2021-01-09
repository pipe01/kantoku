<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.blue-back
        .hero-body
            .container
                h1.title.has-text-light Kantoku

    .sessions.is-flex-grow-1.is-flex.is-align-items-center
        div
            Session
        div
            Session
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import Session from "./Session.vue";

export default defineComponent({
    components: {
        Session
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