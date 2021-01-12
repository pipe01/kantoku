<template lang="pug">
Dashboard(v-if="host" :host="host")
HostSelection(v-else="" @selectedHost="selectedHost")
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import Dashboard from './components/Dashboard.vue'
import HostSelection from './components/HostSelection.vue'

export default defineComponent({
    name: 'App',
    components: {
        Dashboard,
        HostSelection
    },
    setup() {
        const host = ref<string | null>(null);

        function selectedHost(h: string) {
            console.log("use host", h);
            host.value = h;
        }

        return { host, selectedHost }
    }
})
</script>
