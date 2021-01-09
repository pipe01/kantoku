<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.blue-back
        .hero-body
            .container
                h1.title.has-text-light Kantoku

    .is-flex.is-flex-direction-column.is-flex-grow-1.is-justify-content-center.mx-5
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
</style>