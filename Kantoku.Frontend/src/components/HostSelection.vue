<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.is-info
        .hero-body
            span.title.has-text-light.has-text-weight-light Connect to host
</template>

<script lang="ts">
import { defineComponent, inject } from "vue"
import { WebStorage } from "vue-web-storage"

export default defineComponent({
    setup() {
        const storage = inject<WebStorage>("storage")!;

    }
})
</script>