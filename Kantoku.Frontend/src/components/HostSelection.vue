<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.is-info
        .hero-body
            span.title.has-text-light.has-text-weight-light {{isAdding ? "Scan host QR code" : "Connect to host"}}

    .p-3.list-container
        nav.panel.is-flex-grow-1.mb-5(v-if="!isAdding")
            p.panel-heading Known hosts

            a.panel-block(v-for="(host, i) in hosts" @click.self="useHost(host)")
                span(style="margin-right:auto") {{host}}
                button.button(@click="remove(i)")
                    icon(icon="trash")

            a.panel-block.is-unselectable(@click="isAdding = true")
                span.panel-icon
                    icon(icon="plus")
                | Add new

        .is-flex.is-flex-direction-column(v-else="")
            QrScanner(@found="addHost")
        
            button.button.mt-3(@click="isAdding = false")
                span.icon
                    icon(icon="chevron-left")
                span Cancel
</template>

<script lang="ts">
import { defineAsyncComponent, defineComponent, inject, reactive, ref } from "vue"
import { WebStorage } from "vue-web-storage";

export default defineComponent({
    components: {
        QrScanner: defineAsyncComponent(() => import("./QrScanner.vue"))
    },
    setup() {
        const isAdding = ref(false);
        
        const storage = inject<WebStorage>("storage")!
        const hosts = reactive(storage.get<string[]>("hosts") ?? []);

        function save() {
            storage.set("hosts", hosts);
        }

        function remove(i: number) {
            hosts.splice(i, 1);
            save();
        }

        function addHost(host: string | null) {
            isAdding.value = false;

            if (!host) {
                return;
            }

            if (hosts.includes(host)) {
                alert("That host is already on the list!");
            } else {
                hosts.push(host);
                save();
            }
        }

        function useHost(host: string) {
            alert("Use: " + host);
        }

        return { hosts, remove, useHost, isAdding, addHost }
    }
})
</script>

<style lang="scss" scoped>
.list-container {
    overflow-y: scroll;
}
</style>